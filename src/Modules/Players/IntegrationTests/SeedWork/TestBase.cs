using Autofac;
using Autofac.Extensions.DependencyInjection;
using LexiLink.Common.Application;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Time;
using LexiLink.Modules.Players.Infrastructure;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;

namespace LexiLink.Modules.Players.IntegrationTests.SeedWork;

[Category("Integration")]
public abstract class TestBase
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    private static readonly TestExecutionContextAccessor ExecutionContextAccessor = new();
    private static readonly CollectingLogEventSink LogSink = new();
    private static IContainer _container = null!;

    protected ILifetimeScope Scope { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected PlayersContext DbContext { get; private set; } = null!;
    protected TestExecutionContextAccessor ExecutionContext => ExecutionContextAccessor;
    protected CollectingLogEventSink CapturedLogs => LogSink;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ASPNETCORE_LexiLink_IntegrationTests_ConnectionString")
            ?? DefaultConnectionString;

        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventsBus, InMemoryEventsBus>();
        services.AddDbContext<PlayersContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(ExecutionContextAccessor);
        services.AddSingleton<Serilog.ILogger>(
            new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Sink(LogSink)
                .CreateLogger());

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        PlayersStartup.InitializeCompositionRoot(containerBuilder, connectionString);

        _container = containerBuilder.Build();
    }

    [SetUp]
    public async Task SetUp()
    {
        Scope = _container.BeginLifetimeScope();
        Sender = Scope.Resolve<ISender>();
        DbContext = Scope.Resolve<PlayersContext>();

        LogSink.Clear();
        await ClearDatabaseAsync();
    }

    [TearDown]
    public void TearDown()
    {
        Scope?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _container?.Dispose();
    }

    protected Task<TResult> ExecuteCommandAsync<TResult>(IRequest<TResult> command) =>
        Sender.Send(command);

    protected Task ExecuteCommandAsync(IRequest command) => Sender.Send(command);

    protected Task<TResult> ExecuteQueryAsync<TResult>(IRequest<TResult> query) =>
        Sender.Send(query);

    private async Task ClearDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync("""
            DELETE FROM "players"."PlayerAuthIdentities";
            DELETE FROM "players"."Players";
            DELETE FROM "players"."OutboxMessages";
        """);
    }
}
