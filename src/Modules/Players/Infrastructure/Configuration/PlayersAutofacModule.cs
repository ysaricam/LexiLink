using Autofac;
using FluentValidation;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Modules.Players.Application.Configuration;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Application.Configuration.Queries;
using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Players.Infrastructure.Configuration.Processing;
using LexiLink.Modules.Players.Infrastructure.Domain;
using LexiLink.Modules.Players.Infrastructure.Domain.Players;
using LexiLink.Modules.Players.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Players.Infrastructure.Configuration;

public class PlayersAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public PlayersAutofacModule(string connectionString)
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

        builder.RegisterType<PlayersModule>()
            .As<IPlayersModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // UnitOfWork + domain event dispatch infrastructure.
        builder.RegisterType<PlayersDomainEventsDispatcher>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<PlayersUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<OutboxAccessor>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(c => new OutboxProcessor(
                connectionString,
                "players",
                c.Resolve<IDomainNotificationsMapper>(),
                c.Resolve<IPublisher>(),
                c.ResolveOptional<ILogger<OutboxProcessor>>() ?? NullLogger<OutboxProcessor>.Instance,
                c.Resolve<IClock>(),
                c.ResolveOptional<Microsoft.Extensions.Options.IOptions<OutboxProcessingOptions>>()))
            .As<IOutboxProcessor>()
            .InstancePerLifetimeScope();

        // Repositories.
        builder.RegisterType<PlayerRepository>()
            .As<IPlayerRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // Domain services.
        builder.RegisterType<RandomDiscriminatorGenerator>()
            .As<IDiscriminatorGenerator>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // Per-module player context (Kamil-faithful — Application impl, wraps IExecutionContextAccessor).
        builder.RegisterType<PlayerContext>()
            .As<IPlayerContext>()
            .InstancePerLifetimeScope();

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

        builder.RegisterAssemblyTypes(typeof(PlayersAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(PlayersAutofacModule).Assembly)
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
        // Order: innermost first, outermost last → at runtime Logging → Validation → UoW → handler.
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
