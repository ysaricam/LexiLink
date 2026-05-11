using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.API.Configuration.ExecutionContext;
using LexiLink.API.Modules.Games;
using LexiLink.API.Modules.Players;
using LexiLink.Common.Application;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using MediatR;
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

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

var connectionString = builder.Configuration.GetConnectionString("LexiLinkDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'LexiLinkDb' is not configured. " +
        "Set ConnectionStrings__LexiLinkDb env var or populate appsettings.Development.json.");
}

GamesStartup.Initialize(builder.Services, connectionString);
PlayersStartup.Initialize(builder.Services, connectionString);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    GamesStartup.InitializeCompositionRoot(containerBuilder, connectionString);
    PlayersStartup.InitializeCompositionRoot(containerBuilder, connectionString);
});

var app = builder.Build();

GamesStartup.CheckMappings();
PlayersStartup.CheckMappings();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationMiddleware>();
app.UseSerilogRequestLogging();

app.MapGet("/", () => "LexiLink API");

app.MapCategoryEndpoints();
app.MapLinkEndpoints();
app.MapGameEndpoints();
app.MapPlayerEndpoints();

await app.RunAsync();
