using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Common.Infrastructure.Time;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Stats.Application.Contracts;
using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;
using LexiLink.Modules.Stats.Infrastructure.Configuration;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace LexiLink.Modules.Stats.IntegrationTests.SeedWork;

[Category("Integration")]
public abstract class TestBase
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    private static IContainer _container = null!;
    private static string _connectionString = null!;

    protected ILifetimeScope Scope { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected IEventsBus EventsBus { get; private set; } = null!;
    protected IStatsModule StatsModule { get; private set; } = null!;
    protected IReadOnlyCollection<IOutboxProcessor> OutboxProcessors { get; private set; } = null!;
    protected IStatsInboxProcessor StatsInboxProcessor { get; private set; } = null!;
    protected IStatsInternalCommandProcessor StatsInternalCommandProcessor { get; private set; } = null!;
    protected string ConnectionString => _connectionString;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ASPNETCORE_LexiLink_IntegrationTests_ConnectionString")
            ?? DefaultConnectionString;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventsBus, InMemoryEventsBus>();
        GamesStartup.Initialize(services, _connectionString);
        PlayersStartup.Initialize(services, _connectionString);
        StatsStartup.Initialize(services, _connectionString);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(new TestExecutionContextAccessor());
        services.AddSingleton<IAdminAuthorizationContext>(new NoAdminAuthorizationContext());
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);

        // Cross-module gateway stub — Stats integration tests exercise the producer-side
        // event flow; the real Energy module is not booted here.
        services.AddSingleton<IEnergyGuard>(new AlwaysAllowingEnergyGuard());

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        GamesStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        PlayersStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        StatsStartup.InitializeCompositionRoot(containerBuilder, _connectionString);

        _container = containerBuilder.Build();
    }

    [SetUp]
    public async Task SetUp()
    {
        Scope = _container.BeginLifetimeScope();
        Sender = Scope.Resolve<ISender>();
        EventsBus = Scope.Resolve<IEventsBus>();
        StatsModule = Scope.Resolve<IStatsModule>();
        OutboxProcessors = Scope.Resolve<IEnumerable<IOutboxProcessor>>().ToArray();
        StatsInboxProcessor = Scope.Resolve<IStatsInboxProcessor>();
        StatsInternalCommandProcessor = Scope.Resolve<IStatsInternalCommandProcessor>();

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

    protected async Task ProcessOutboxAsync()
    {
        foreach (var processor in OutboxProcessors)
        {
            await processor.ProcessAsync();
        }
    }

    protected Task ProcessStatsInboxAsync() => StatsInboxProcessor.ProcessAsync();

    protected Task ProcessStatsInternalCommandsAsync() => StatsInternalCommandProcessor.ProcessAsync();

    private sealed class AlwaysAllowingEnergyGuard : IEnergyGuard
    {
        public Task EnsureCanStartGameAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private static async Task ClearDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            DELETE FROM "administration"."AdminActionAudit";
            DELETE FROM "stats"."InternalCommands";
            DELETE FROM "stats"."InboxMessages";
            DELETE FROM "stats"."PlayerPeriodStats";
            DELETE FROM "stats"."PlayerStats";
            DELETE FROM "games"."GameHistory";
            DELETE FROM "games"."GameOptimalPath";
            DELETE FROM "games"."Games";
            DELETE FROM "games"."LinkOutgoingLinks";
            DELETE FROM "games"."Links";
            DELETE FROM "games"."Categories";
            DELETE FROM "games"."OutboxMessages";
            DELETE FROM "players"."PlayerAuthIdentities";
            DELETE FROM "players"."Players";
            DELETE FROM "players"."OutboxMessages";
        """);
    }
}
