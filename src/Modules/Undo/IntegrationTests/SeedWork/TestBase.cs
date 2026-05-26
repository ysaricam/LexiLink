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
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Infrastructure;
using LexiLink.Modules.Undo.Infrastructure.Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace LexiLink.Modules.Undo.IntegrationTests.SeedWork;

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
    protected IUndoModule UndoModule { get; private set; } = null!;
    protected UndoContext DbContext { get; private set; } = null!;
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
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventsBus, InMemoryEventsBus>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        PlayersStartup.Initialize(services, _connectionString);
        UndoStartup.Initialize(services, _connectionString);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(new TestExecutionContextAccessor());
        services.AddSingleton<TestAdminAuthorizationContext>();
        services.AddSingleton<IAdminAuthorizationContext>(sp =>
            sp.GetRequiredService<TestAdminAuthorizationContext>());
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        PlayersStartup.InitializeCompositionRoot(containerBuilder, _connectionString);
        UndoStartup.InitializeCompositionRoot(containerBuilder, _connectionString);

        _container = containerBuilder.Build();
    }

    [SetUp]
    public async Task SetUp()
    {
        Scope = _container.BeginLifetimeScope();
        Sender = Scope.Resolve<ISender>();
        EventsBus = Scope.Resolve<IEventsBus>();
        UndoModule = Scope.Resolve<IUndoModule>();
        DbContext = Scope.Resolve<UndoContext>();
        OutboxProcessors = Scope.Resolve<IEnumerable<IOutboxProcessor>>().ToArray();
        AdminContext = Scope.Resolve<TestAdminAuthorizationContext>();
        AdminContext.Logout();

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

    protected async Task ExecuteSqlAsync(string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
        DbContext.ChangeTracker.Clear();
    }

    private static async Task ClearDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync("""
            DELETE FROM "undo"."OutboxMessages";
            DELETE FROM "undo"."PlayerUndoInventories";
            DELETE FROM "players"."PlayerAuthIdentities";
            DELETE FROM "players"."Players";
            DELETE FROM "players"."OutboxMessages";
        """);
    }
}
