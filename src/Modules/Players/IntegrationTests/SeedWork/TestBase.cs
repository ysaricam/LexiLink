using Autofac;
using Autofac.Extensions.DependencyInjection;
using LexiLink.Common.Application;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Players.Infrastructure;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Players.IntegrationTests.SeedWork;

[Category("Integration")]
public abstract class TestBase
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    private static IContainer _container = null!;

    protected ILifetimeScope Scope { get; private set; } = null!;
    protected ISender Sender { get; private set; } = null!;
    protected PlayersContext DbContext { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ASPNETCORE_LexiLink_IntegrationTests_ConnectionString")
            ?? DefaultConnectionString;

        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddDbContext<PlayersContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(new TestExecutionContextAccessor());
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        containerBuilder.RegisterModule(new PlayersAutofacModule(connectionString));
        var notificationsMap = new BiDictionary<string, Type>();
        containerBuilder.RegisterModule(new OutboxModule(notificationsMap));

        _container = containerBuilder.Build();
    }

    [SetUp]
    public async Task SetUp()
    {
        Scope = _container.BeginLifetimeScope();
        Sender = Scope.Resolve<ISender>();
        DbContext = Scope.Resolve<PlayersContext>();

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
