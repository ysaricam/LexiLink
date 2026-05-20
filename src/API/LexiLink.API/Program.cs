using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.Bootstrap;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.API.Configuration.ExecutionContext;
using LexiLink.API.Configuration.Health;
using LexiLink.API.Configuration.Inbox;
using LexiLink.API.Configuration.OpenApi;
using LexiLink.API.Configuration.Operations;
using LexiLink.API.Configuration.Outbox;
using LexiLink.API.CrossModule;
using LexiLink.API.Modules.Admin;
using LexiLink.API.Modules.Auth;
using LexiLink.API.Modules.Energy;
using LexiLink.API.Modules.Games;
using LexiLink.API.Modules.Operations;
using LexiLink.API.Modules.Players;
using LexiLink.API.Modules.Quests;
using LexiLink.API.Modules.Stats;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Modules.Administration.Infrastructure.Configuration;
using LexiLink.Modules.Energy.Infrastructure.Configuration;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Quests.Infrastructure.Configuration;
using LexiLink.Modules.Stats.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Quartz;
using Scalar.AspNetCore;
using Serilog;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

ILogger logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

Log.Logger = logger;
builder.Host.UseSerilog(logger);
builder.Services.AddSingleton(logger);
builder.Services.AddSingleton<IClock, LexiLink.Common.Infrastructure.Time.SystemClock>();
builder.Services.AddScoped<IEventsBus, InMemoryEventsBus>();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

var connectionString = builder.Configuration.GetConnectionString("LexiLinkDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'LexiLinkDb' is not configured. " +
        "Set ConnectionStrings__LexiLinkDb env var or populate appsettings.Development.json.");
}

var databaseScriptsDirectory = Path.Combine(AppContext.BaseDirectory, "Database", "Structure");

GamesStartup.Initialize(builder.Services, connectionString);
PlayersStartup.Initialize(builder.Services, connectionString);
StatsStartup.Initialize(builder.Services, connectionString);
EnergyStartup.Initialize(builder.Services, connectionString);
QuestsStartup.Initialize(builder.Services, connectionString);
AdministrationStartup.Initialize(builder.Services, connectionString);
builder.Services.Configure<AdministrationBootstrapOptions>(
    builder.Configuration.GetSection(AdministrationBootstrapOptions.SectionName));
builder.Services.AddHostedService<AdministrationBootstrapHostedService>();
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .Add(new HealthCheckRegistration(
        "postgresql",
        _ => new LexiLinkDatabaseHealthCheck(connectionString),
        HealthStatus.Unhealthy,
        tags: ["ready", "db"]))
    .Add(new HealthCheckRegistration(
        "database-migrations",
        _ => new LexiLinkMigrationHealthCheck(connectionString, databaseScriptsDirectory),
        HealthStatus.Unhealthy,
        tags: ["ready", "db"]));
builder.Services.AddSingleton(sp => new ProcessorBacklogReader(
    connectionString,
    sp.GetRequiredService<IOptions<OutboxProcessingOptions>>(),
    sp.GetRequiredService<IClock>()));
builder.Services.Configure<OutboxProcessingOptions>(
    builder.Configuration.GetSection("OutboxProcessing"));
var outboxProcessingOptions =
    builder.Configuration.GetSection("OutboxProcessing").Get<OutboxProcessingOptions>()
    ?? new OutboxProcessingOptions();
builder.Services.AddQuartz(configurator =>
{
    var outboxJobKey = new JobKey("ProcessOutboxMessages");

    configurator.AddJob<ProcessOutboxMessagesJob>(job => job.WithIdentity(outboxJobKey));
    configurator.AddTrigger(trigger => trigger
        .ForJob(outboxJobKey)
        .WithIdentity("ProcessOutboxMessagesTrigger")
        .StartNow()
        .WithSimpleSchedule(schedule => schedule
            .WithInterval(outboxProcessingOptions.PollingInterval)
            .RepeatForever()));

    var statsInboxJobKey = new JobKey("ProcessStatsInboxMessages");

    configurator.AddJob<ProcessStatsInboxMessagesJob>(job => job.WithIdentity(statsInboxJobKey));
    configurator.AddTrigger(trigger => trigger
        .ForJob(statsInboxJobKey)
        .WithIdentity("ProcessStatsInboxMessagesTrigger")
        .StartNow()
        .WithSimpleSchedule(schedule => schedule
            .WithInterval(outboxProcessingOptions.PollingInterval)
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();
var authOptions = builder.Configuration.GetSection("Authentication").Get<LexiLinkAuthOptions>()
    ?? new LexiLinkAuthOptions();
LexiLinkAuthOptionsValidator.Validate(authOptions, builder.Environment);
var allowedCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("LexiLinkFrontend", policy =>
    {
        if (allowedCorsOrigins.Length == 0)
        {
            policy.DisallowCredentials();
            return;
        }

        policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton(authOptions);
builder.Services
    .AddAuthentication(AuthConstants.Scheme)
    .AddScheme<AuthenticationSchemeOptions, LexiLinkBearerAuthenticationHandler>(
        AuthConstants.Scheme,
        _ => { });
builder.Services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
builder.Services.AddSingleton<IExternalIdentityVerifier>(
    authOptions.TokenExchange.Mode == ExternalIdentityValidationMode.DevelopmentExternalToken
        ? new DevelopmentExternalIdentityVerifier()
        : new DisabledExternalIdentityVerifier());
builder.Services.AddSingleton<IExternalAdminIdentityVerifier>(
    authOptions.AdminTokenExchange.Mode == ExternalIdentityValidationMode.DevelopmentExternalToken
        ? new DevelopmentExternalAdminIdentityVerifier()
        : new DisabledExternalAdminIdentityVerifier());
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthConstants.AuthenticatedPlayerPolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(AuthConstants.Scheme);
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy(AuthConstants.AuthenticatedAdminPolicy, policy =>
    {
        policy.AuthenticationSchemes.Add(AuthConstants.Scheme);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthConstants.RoleClaimType, AuthConstants.AdminRoleValue);
    });

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(LexiLinkOpenApiTransformers.AddBearerSecuritySchemeAsync);
    options.AddOperationTransformer(LexiLinkOpenApiTransformers.AddBearerSecurityRequirementAsync);
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    GamesStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    PlayersStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    StatsStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    EnergyStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    QuestsStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    AdministrationStartup.InitializeCompositionRoot(containerBuilder, connectionString);

    containerBuilder.RegisterType<EnergyGuard>()
        .As<IEnergyGuard>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<AdminLookup>()
        .As<IAdminLookup>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<PlayerStatusLookup>()
        .As<IPlayerStatusLookup>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<AdminAuthorizationContext>()
        .As<IAdminAuthorizationContext>()
        .InstancePerLifetimeScope();
});

var app = builder.Build();

GamesStartup.CheckMappings();
PlayersStartup.CheckMappings();
EnergyStartup.CheckMappings();
QuestsStartup.CheckMappings();
AdministrationStartup.CheckMappings();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("LexiLinkFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "LexiLink API");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
}).AllowAnonymous();

app.MapAuthEndpoints();
app.MapAdminAuthEndpoints();
app.MapAdminAuditEndpoints();
app.MapAdminQuestEndpoints();
app.MapAdminEnergyEndpoints();
app.MapAdminPlayerEndpoints();
app.MapCategoryEndpoints();
app.MapLinkEndpoints();
app.MapGameEndpoints();
app.MapPlayerEndpoints();
app.MapStatsEndpoints();
app.MapEnergyEndpoints();
app.MapQuestEndpoints();
app.MapOperationsEndpoints();

await app.RunAsync();

public partial class Program;
