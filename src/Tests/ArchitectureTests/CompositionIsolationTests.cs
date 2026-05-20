using Autofac;
using Autofac.Extensions.DependencyInjection;
using LexiLink.Common.Application;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Common.Infrastructure.IntegrationEvents;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Common.Infrastructure.Time;
using LexiLink.Modules.Administration.Infrastructure.Configuration;
using LexiLink.Modules.Games.Infrastructure.Configuration;
using LexiLink.Modules.Players.Infrastructure.Configuration;
using LexiLink.Modules.Stats.Infrastructure.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.ArchitectureTests;

[TestFixture]
public class CompositionIsolationTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=lexilink;Username=lexiadmin;Password=0852";

    [Test]
    public void SharedContainer_Should_NotExposeModuleOwnedInfrastructureAsCommonServices()
    {
        using var container = BuildContainer();
        using var scope = container.BeginLifetimeScope();

        scope.ResolveOptional<DbContext>().Should().BeNull();
        scope.ResolveOptional<IUnitOfWork>().Should().BeNull();
        scope.ResolveOptional<IDomainEventsDispatcher>().Should().BeNull();
        scope.ResolveOptional<Common.Application.Outbox.IOutbox>().Should().BeNull();

        scope.Resolve<IEnumerable<IOutboxProcessor>>().Should().HaveCount(3);
    }

    [Test]
    public void EventsBus_Should_BeScopedToTheCurrentLifetimeScope()
    {
        using var container = BuildContainer();
        using var firstScope = container.BeginLifetimeScope();
        using var secondScope = container.BeginLifetimeScope();

        var firstResolve = firstScope.Resolve<IEventsBus>();
        var secondResolveInSameScope = firstScope.Resolve<IEventsBus>();
        var secondScopeResolve = secondScope.Resolve<IEventsBus>();

        firstResolve.Should().BeSameAs(secondResolveInSameScope);
        firstResolve.Should().NotBeSameAs(secondScopeResolve);
    }

    private static IContainer BuildContainer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventsBus, InMemoryEventsBus>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediator).Assembly));
        services.AddSingleton<IExecutionContextAccessor>(new UnavailableExecutionContextAccessor());
        services.AddSingleton<Serilog.ILogger>(Serilog.Core.Logger.None);

        GamesStartup.Initialize(services, ConnectionString);
        PlayersStartup.Initialize(services, ConnectionString);
        StatsStartup.Initialize(services, ConnectionString);
        AdministrationStartup.Initialize(services, ConnectionString);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        GamesStartup.InitializeCompositionRoot(containerBuilder, ConnectionString);
        PlayersStartup.InitializeCompositionRoot(containerBuilder, ConnectionString);
        StatsStartup.InitializeCompositionRoot(containerBuilder, ConnectionString);
        AdministrationStartup.InitializeCompositionRoot(containerBuilder, ConnectionString);

        return containerBuilder.Build();
    }

    private sealed class UnavailableExecutionContextAccessor : IExecutionContextAccessor
    {
        public Guid UserId => throw new InvalidOperationException("Execution context is not available.");

        public Guid CorrelationId => Guid.Empty;

        public bool IsAvailable => false;

        public bool IsAdmin => false;

        public Guid? AdminUserId => null;
    }
}
