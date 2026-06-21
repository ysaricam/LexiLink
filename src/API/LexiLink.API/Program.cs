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
using LexiLink.API.Modules.Ads;
using LexiLink.API.Modules.Auth;
using LexiLink.API.Modules.Diamond;
using LexiLink.API.Modules.Energy;
using LexiLink.API.Modules.Games;
using LexiLink.API.Modules.Hint;
using LexiLink.API.Modules.Market;
using LexiLink.API.Modules.Operations;
using LexiLink.API.Modules.Payments;
using LexiLink.API.Modules.Players;
using LexiLink.API.Modules.Quests;
using LexiLink.API.Modules.Reset;
using LexiLink.API.Modules.Stats;
using LexiLink.API.Modules.Undo;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Administration.Infrastructure.Configuration;
using LexiLink.Modules.Ads.Application.Configuration.Verification;
using LexiLink.Modules.Ads.Infrastructure.Configuration;
using LexiLink.Modules.Ads.Infrastructure.Configuration.Verification;
using LexiLink.Modules.Diamond.Infrastructure.Configuration;
using LexiLink.Modules.Energy.Application.Configuration.CrossModule;
using LexiLink.Modules.Energy.Infrastructure.Configuration;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Hint.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using LexiLink.Modules.Hint.Infrastructure.Configuration;
using LexiLink.Modules.Market.Infrastructure.Configuration;
using LexiLink.Modules.Payments.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;
using LexiLink.Modules.Quests.Infrastructure.Configuration;
using LexiLink.Modules.Reset.Application.Configuration.CrossModule;
using LexiLink.Modules.Reset.Infrastructure.Configuration;
using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Infrastructure.Configuration;
using LexiLink.Modules.Undo.Application.Configuration.CrossModule;
using LexiLink.Modules.Undo.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Quartz;
using Scalar.AspNetCore;
using Serilog;
using ILogger = Serilog.ILogger;

// Npgsql 6+ defaults to converting Kind=Utc DateTime values into the
// session's local timezone when writing to a "timestamp without time
// zone" column. Our schema uses that column type but the application
// writes DateTime.UtcNow everywhere, so the round-trip silently
// shifts timestamps by the local offset. This breaks consumers that
// re-tag the read value as UTC (e.g. EnergyRefillCalculator), making
// the bucket appear fully refilled after one offset's worth of time.
// The legacy behavior writes UTC values verbatim into the column.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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
DiamondStartup.Initialize(builder.Services, connectionString);
AdsStartup.Initialize(builder.Services, connectionString);
HintStartup.Initialize(builder.Services, connectionString);
UndoStartup.Initialize(builder.Services, connectionString);
ResetStartup.Initialize(builder.Services, connectionString);
MarketStartup.Initialize(builder.Services, connectionString);
PaymentsStartup.Initialize(builder.Services, connectionString);
builder.Services.Configure<AppleIapOptions>(
    builder.Configuration.GetSection(AppleIapOptions.SectionName));
builder.Services.Configure<GooglePlayIapOptions>(
    builder.Configuration.GetSection(GooglePlayIapOptions.SectionName));
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
    authOptions.TokenExchange.Mode switch
    {
        ExternalIdentityValidationMode.DevelopmentExternalToken => new DevelopmentExternalIdentityVerifier(),
        ExternalIdentityValidationMode.GuestDevice => new GuestExternalIdentityVerifier(),
        ExternalIdentityValidationMode.GuestDeviceAndSocial => new SocialExternalIdentityVerifier(authOptions),
        _ => new DisabledExternalIdentityVerifier()
    });
builder.Services.AddSingleton<IExternalAdminIdentityVerifier>(
    authOptions.AdminTokenExchange.Mode switch
    {
        ExternalIdentityValidationMode.DevelopmentExternalToken => new DevelopmentExternalAdminIdentityVerifier(),
        ExternalIdentityValidationMode.AdminSharedSecret => new SharedSecretExternalAdminIdentityVerifier(
            authOptions.AdminTokenExchange.SharedSecret!),
        _ => new DisabledExternalAdminIdentityVerifier()
    });
var adsSsvOptions = builder.Configuration.GetSection(AdsSsvOptions.SectionName).Get<AdsSsvOptions>()
    ?? new AdsSsvOptions();
builder.Services.AddSingleton<IAdMobSsvVerifier>(
    adsSsvOptions.Mode == AdsSsvVerificationMode.DevelopmentFailOpen
        ? new DevelopmentAdMobSsvVerifier()
        : new AdMobSsvVerifier(adsSsvOptions));
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
    DiamondStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    AdsStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    HintStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    UndoStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    ResetStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    MarketStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    PaymentsStartup.InitializeCompositionRoot(containerBuilder, connectionString);

    containerBuilder.RegisterType<EnergyGuard>()
        .As<IEnergyGuard>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<HintGuard>()
        .As<IHintGuard>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<UndoGuard>()
        .As<IUndoGuard>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<ResetGuard>()
        .As<IResetGuard>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<DiamondGuard>()
        .As<IDiamondGuard>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<DiamondGrant>()
        .As<IDiamondGrant>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<EnergyGrant>()
        .As<IEnergyGrant>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<HintGrant>()
        .As<IHintGrant>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<UndoGrant>()
        .As<IUndoGrant>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<ResetGrant>()
        .As<IResetGrant>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<AdminLookup>()
        .As<IAdminLookup>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<PlayerStatusLookup>()
        .As<IPlayerStatusLookup>()
        .InstancePerLifetimeScope();

    containerBuilder.Register(c => new QuestCounterReader(
            connectionString,
            c.Resolve<IEnumerable<IOutboxProcessor>>(),
            c.Resolve<IStatsInternalCommandScheduler>(),
            c.Resolve<IStatsInternalCommandProcessor>()))
        .As<IQuestCounterReader>()
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
DiamondStartup.CheckMappings();
AdsStartup.CheckMappings();
HintStartup.CheckMappings();
UndoStartup.CheckMappings();
ResetStartup.CheckMappings();
MarketStartup.CheckMappings();
PaymentsStartup.CheckMappings();

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
app.MapAdminHintEndpoints();
app.MapAdminUndoEndpoints();
app.MapAdminResetEndpoints();
app.MapAdminDiamondEndpoints();
app.MapAdminMarketEndpoints();
app.MapAdminPaymentsEndpoints();
app.MapAdminPlayerEndpoints();
app.MapAdminContentEndpoints();
app.MapCategoryEndpoints();
app.MapLinkEndpoints();
app.MapGameEndpoints();
app.MapPlayerEndpoints();
app.MapStatsEndpoints();
app.MapEnergyEndpoints();
app.MapHintEndpoints();
app.MapUndoEndpoints();
app.MapResetEndpoints();
app.MapDiamondEndpoints();
app.MapMarketEndpoints();
app.MapPaymentsEndpoints();
app.MapAdsEndpoints();
app.MapQuestEndpoints();
app.MapOperationsEndpoints();

await app.RunAsync();

public partial class Program;
