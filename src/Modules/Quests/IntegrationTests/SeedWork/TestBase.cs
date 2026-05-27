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
using LexiLink.Modules.Administration.Infrastructure.Configuration;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Infrastructure.Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace LexiLink.Modules.Quests.IntegrationTests.SeedWork;

[Category("Integration")]
public abstract class TestBase
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    // Daily seed id from 021_SeedQuestDefinitions.sql. Test base keeps the
    // daily quest in place between tests; admin-created definitions are
    // wiped in ClearDatabaseAsync.
    protected static readonly Guid SeedDailyQuestDefinitionId =
        Guid.Parse("11111111-0000-0000-0000-000000000010");

    private static IContainer _container = null!;
    private static string _connectionString = null!;

    protected ILifetimeScope Scope { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected IEventsBus EventsBus { get; private set; } = null!;
    protected IQuestsModule QuestsModule { get; private set; } = null!;
    protected IReadOnlyCollection<IOutboxProcessor> OutboxProcessors { get; private set; } = null!;
    protected TestAdminAuthorizationContext AdminContext { get; private set; } = null!;
    protected MutableQuestCounterReader QuestCounterReader { get; private set; } = null!;
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
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        GamesStartup.Initialize(services, _connectionString);
        PlayersStartup.Initialize(services, _connectionString);
        QuestsStartup.Initialize(services, _connectionString);
        AdministrationStartup.Initialize(services, _connectionString);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(new TestExecutionContextAccessor());
        services.AddSingleton<TestAdminAuthorizationContext>();
        services.AddSingleton<IAdminAuthorizationContext>(sp =>
            sp.GetRequiredService<TestAdminAuthorizationContext>());
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);

        // Cross-module gateway stubs — Quests IT doesn't boot resource modules
        // and doesn't depend on Stats / Players counters being populated.
        services.AddSingleton<IEnergyGuard>(new AlwaysAllowingEnergyGuard());
        services.AddSingleton<IHintGuard>(new AlwaysAllowingHintGuard());
        services.AddSingleton<IUndoGuard>(new AlwaysAllowingUndoGuard());
        services.AddSingleton<IResetGuard>(new AlwaysAllowingResetGuard());
        services.AddSingleton<MutableQuestCounterReader>();
        services.AddSingleton<IQuestCounterReader>(sp => sp.GetRequiredService<MutableQuestCounterReader>());

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        GamesStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        PlayersStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        QuestsStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        AdministrationStartup.InitializeCompositionRoot(containerBuilder, _connectionString);

        _container = containerBuilder.Build();
    }

    [SetUp]
    public async Task SetUp()
    {
        Scope = _container.BeginLifetimeScope();
        Sender = Scope.Resolve<ISender>();
        EventsBus = Scope.Resolve<IEventsBus>();
        QuestsModule = Scope.Resolve<IQuestsModule>();
        AdminContext = Scope.Resolve<TestAdminAuthorizationContext>();
        AdminContext.Logout();
        OutboxProcessors = Scope.Resolve<IEnumerable<IOutboxProcessor>>().ToArray();
        QuestCounterReader = Scope.Resolve<MutableQuestCounterReader>();
        QuestCounterReader.Reset();

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

    protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
    }

    protected async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return (await connection.QueryAsync<T>(sql, parameters)).ToList();
    }

    private sealed class AlwaysAllowingEnergyGuard : IEnergyGuard
    {
        public Task EnsureCanStartGameAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class AlwaysAllowingHintGuard : IHintGuard
    {
        public Task EnsureHintAvailableAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class AlwaysAllowingUndoGuard : IUndoGuard
    {
        public Task EnsureUndoAvailableAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class AlwaysAllowingResetGuard : IResetGuard
    {
        public Task EnsureResetAvailableAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private static async Task ClearDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync($"""
            DELETE FROM "administration"."AdminActionAudit";
            DELETE FROM "quests"."OutboxMessages";
            DELETE FROM "quests"."PlayerQuests" WHERE TRUE;
            -- Remove admin-created QuestDefinitions; keep the seeded daily
            -- quest (021_SeedQuestDefinitions.sql) so per-test setup is
            -- deterministic.
            DELETE FROM "quests"."QuestDefinitions"
                WHERE "Id" <> '11111111-0000-0000-0000-000000000010';
            -- Reset the seeded daily quest in case an Update mutated it.
            UPDATE "quests"."QuestDefinitions"
                SET "IsActive" = TRUE,
                    "Name" = 'Günlük 3 Oyun',
                    "Description" = 'Bugün 3 oyun tamamla.',
                    "Trigger" = 'GameCompletedDaily',
                    "Threshold" = 3,
                    "EnergyReward" = 5,
                    "HintReward" = 0,
                    "UndoReward" = 0,
                    "ResetReward" = 0,
                    "DiamondReward" = 0,
                    "PrerequisiteQuestDefinitionId" = NULL,
                    "ProgressBaseline" = 'FromSnapshot'
                WHERE "Id" = '11111111-0000-0000-0000-000000000010';
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

/// <summary>
/// Test-only counter reader. Quests Application + Infrastructure depend
/// on <see cref="IQuestCounterReader"/> for issuance baselines and
/// claim eligibility. Production reads from <c>stats.*</c> +
/// <c>players.*</c>, but the Quests IT container does not boot Stats /
/// Players. This stub lets each test simulate the counters it cares
/// about; defaults to all zeros.
/// </summary>
public sealed class MutableQuestCounterReader : IQuestCounterReader
{
    public int GamesCompletedTotal { get; set; }
    public int GamesCompletedToday { get; set; }
    public bool AuthProviderLinked { get; set; }

    public Task<QuestCounters> ReadAsync(
        Guid playerId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new QuestCounters(
            GamesCompletedTotal,
            GamesCompletedToday,
            AuthProviderLinked));

    public void Reset()
    {
        GamesCompletedTotal = 0;
        GamesCompletedToday = 0;
        AuthProviderLinked = false;
    }
}
