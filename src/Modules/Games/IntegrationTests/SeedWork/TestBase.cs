using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Common.Infrastructure.Time;
using LexiLink.Modules.Administration.Infrastructure.Configuration;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Infrastructure;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using Npgsql;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Games.IntegrationTests.SeedWork;

[Category("Integration")]
public abstract class TestBase
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    private static IContainer _container = null!;
    private static string _connectionString = null!;

    protected ILifetimeScope Scope { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected GamesContext DbContext { get; private set; } = null!;
    protected IReadOnlyCollection<IOutboxProcessor> OutboxProcessors { get; private set; } = null!;
    protected TestAdminAuthorizationContext AdminContext { get; private set; } = null!;
    protected string ConnectionString => _connectionString;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ASPNETCORE_LexiLink_IntegrationTests_ConnectionString")
            ?? DefaultConnectionString;

        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventsBus, InMemoryEventsBus>();
        services.AddDbContext<GamesContext>(opts =>
            opts.UseNpgsql(_connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
        AdministrationStartup.Initialize(services, _connectionString);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(new TestExecutionContextAccessor());
        services.AddSingleton<TestAdminAuthorizationContext>();
        services.AddSingleton<IAdminAuthorizationContext>(sp =>
            sp.GetRequiredService<TestAdminAuthorizationContext>());
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);

        // Cross-module gateway stubs — Games integration tests exercise the Games
        // module in isolation; the real Energy and Hint modules are not booted here.
        // The HintGuard stub is recording + configurable: by default it allows
        // every call, but per-test code can flip it to reject (RejectNext flag)
        // and inspect the CallCount. Tests still use the default unless they
        // resolve and reconfigure it.
        services.AddSingleton<IEnergyGuard>(new AlwaysAllowingEnergyGuard());
        services.AddSingleton<RecordingHintGuard>();
        services.AddSingleton<IHintGuard>(sp => sp.GetRequiredService<RecordingHintGuard>());

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        GamesStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        AdministrationStartup.InitializeCompositionRoot(containerBuilder, _connectionString);

        _container = containerBuilder.Build();
    }

    /// <summary>
    /// Stable admin id Games.IT runs every test as by default. B10 moved
    /// content commands (CreateCategory / CreateLink / Activate /
    /// Deactivate / Add+Remove edges) under IAdminCommand, so existing
    /// tests that seed graphs through those commands need an admin
    /// principal in scope. ContentAdminCommandTests calls
    /// <see cref="TestAdminAuthorizationContext.Logout"/> when it needs
    /// the non-admin path.
    /// </summary>
    public static readonly Guid DefaultAdminId =
        Guid.Parse("99999999-0000-0000-0000-000000000001");

    protected RecordingHintGuard HintGuard { get; private set; } = null!;

    [SetUp]
    public async Task SetUp()
    {
        Scope = _container.BeginLifetimeScope();
        Sender = Scope.Resolve<ISender>();
        DbContext = Scope.Resolve<GamesContext>();
        OutboxProcessors = Scope.Resolve<IEnumerable<IOutboxProcessor>>().ToArray();
        AdminContext = Scope.Resolve<TestAdminAuthorizationContext>();
        AdminContext.LoginAs(DefaultAdminId);
        HintGuard = Scope.Resolve<RecordingHintGuard>();
        HintGuard.Reset();

        await ClearDatabaseAsync();
    }

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

    private sealed class AlwaysAllowingEnergyGuard : IEnergyGuard
    {
        public Task EnsureCanStartGameAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    /// <summary>
    /// Per-test configurable HintGuard stub. Resolves as a singleton so
    /// that mutating <see cref="RejectNext"/> from a test affects the
    /// same instance the command handler sees through <c>IHintGuard</c>.
    /// </summary>
    public sealed class RecordingHintGuard : IHintGuard
    {
        public int CallCount { get; private set; }
        public bool RejectNext { get; set; }

        public Task EnsureHintAvailableAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (RejectNext)
            {
                throw new InvalidOperationException(
                    "Stubbed IHintGuard configured to reject this call.");
            }
            return Task.CompletedTask;
        }

        public void Reset()
        {
            CallCount = 0;
            RejectNext = false;
        }
    }

    private async Task ClearDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync(@"
            DELETE FROM ""administration"".""AdminActionAudit"";
            DELETE FROM ""games"".""GameOptimalPath"";
            DELETE FROM ""games"".""GameHistory"";
            DELETE FROM ""games"".""Games"";
            DELETE FROM ""games"".""LinkOutgoingLinks"";
            DELETE FROM ""games"".""Links"";
            DELETE FROM ""games"".""Categories"";
            DELETE FROM ""games"".""OutboxMessages"";
        ");
    }
}
