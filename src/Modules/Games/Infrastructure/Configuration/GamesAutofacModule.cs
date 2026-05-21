using Autofac;
using FluentValidation;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using LexiLink.Modules.Games.Application.Contracts;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;
using LexiLink.Modules.Games.Infrastructure.Configuration.Processing;
using LexiLink.Modules.Games.Infrastructure.Domain;
using LexiLink.Modules.Games.Infrastructure.Domain.Categories;
using LexiLink.Modules.Games.Infrastructure.Domain.Games;
using LexiLink.Modules.Games.Infrastructure.Domain.Links;
using LexiLink.Modules.Games.Infrastructure.Domain.Services;
using LexiLink.Modules.Games.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Games.Infrastructure.Configuration;

public class GamesAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public GamesAutofacModule(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void Load(ContainerBuilder builder)
    {
        var connectionString = _connectionString;

        builder.Register(_ => new SqlConnectionFactory(connectionString))
            .As<ISqlConnectionFactory>()
            .InstancePerLifetimeScope();


        var applicationAssembly = Assemblies.Application;
        var allCtors = new AllConstructorFinder();

        builder.RegisterType<GamesModule>()
            .As<IGamesModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // UnitOfWork + domain event dispatch infrastructure.
        builder.RegisterType<GamesDomainEventsDispatcher>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<GamesUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<OutboxAccessor>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(c => new OutboxProcessor(
                connectionString,
                "games",
                c.Resolve<IDomainNotificationsMapper>(),
                c.Resolve<IPublisher>(),
                c.ResolveOptional<ILogger<OutboxProcessor>>() ?? NullLogger<OutboxProcessor>.Instance,
                c.Resolve<IClock>(),
                c.ResolveOptional<Microsoft.Extensions.Options.IOptions<OutboxProcessingOptions>>()))
            .As<IOutboxProcessor>()
            .InstancePerLifetimeScope();

        // Repositories.
        builder.RegisterType<CategoryRepository>()
            .As<ICategoryRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<LinkRepository>()
            .As<ILinkRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<GameRepository>()
            .As<IGameRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<CompletedGameLinkPairRepository>()
            .As<ICompletedGameLinkPairRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // Domain services.
        builder.RegisterType<StandardScoreCalculator>()
            .As<IScoreCalculator>()
            .SingleInstance();

        builder.RegisterType<StandardGameConfigurationService>()
            .As<IGameConfigurationService>()
            .SingleInstance();

        builder.RegisterType<PathFinderService>()
            .As<IPathFinderService>()
            .SingleInstance();

        builder.RegisterType<LinkNeighborResolver>()
            .As<ILinkNeighborResolver>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(_ => new Random())
            .AsSelf()
            .SingleInstance();

        // Handlers (assembly scan). AsImplementedInterfaces makes each handler resolvable as
        // both ICommandHandler<T> (decorator target) and IRequestHandler<T> (MediatR target).
        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(ICommandHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(IQueryHandler<,>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(IValidator<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(GamesAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(GamesAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(IDomainEventNotification<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // Decorator chain — all stacked on IRequestHandler<>/<,> so MediatR's resolution path
        // runs the full chain. The decorators implement ICommandHandler<T> (which extends
        // IRequestHandler<T>), so the previous registration auto-fills the next decorator's
        // `ICommandHandler<T> decorated` constructor slot via runtime cast.
        // Constraint `where T : ICommand[<TResult>]` filters queries out — query handlers
        // pass through the bare IRequestHandler<,> registration unchanged.
        // Order: innermost first, outermost last → at runtime Logging → Validation → UoW → AdminAuditing → handler.
        // AdminAuditing must be the INNERMOST decorator so the audit
        // outbox row commits in the same SaveChangesAsync as the
        // command's domain changes. See B7 commit for design rationale.
        builder.RegisterGenericDecorator(
            typeof(AdminAuditingCommandHandlerDecorator<>),
            typeof(IRequestHandler<>));

        builder.RegisterGenericDecorator(
            typeof(AdminAuditingCommandHandlerWithResultDecorator<,>),
            typeof(IRequestHandler<,>));

        builder.RegisterGenericDecorator(
            typeof(UnitOfWorkCommandHandlerDecorator<>),
            typeof(IRequestHandler<>));

        builder.RegisterGenericDecorator(
            typeof(UnitOfWorkCommandHandlerWithResultDecorator<,>),
            typeof(IRequestHandler<,>));

        builder.RegisterGenericDecorator(
            typeof(ValidationCommandHandlerDecorator<>),
            typeof(IRequestHandler<>));

        builder.RegisterGenericDecorator(
            typeof(ValidationCommandHandlerWithResultDecorator<,>),
            typeof(IRequestHandler<,>));

        builder.RegisterGenericDecorator(
            typeof(LoggingCommandHandlerDecorator<>),
            typeof(IRequestHandler<>));

        builder.RegisterGenericDecorator(
            typeof(LoggingCommandHandlerWithResultDecorator<,>),
            typeof(IRequestHandler<,>));

    }
}
