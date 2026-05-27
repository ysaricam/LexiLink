using Autofac;
using FluentValidation;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Application.Configuration.Queries;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;
using LexiLink.Modules.Diamond.Infrastructure.Configuration.Processing;
using LexiLink.Modules.Diamond.Infrastructure.Domain;
using LexiLink.Modules.Diamond.Infrastructure.Domain.PlayerDiamondInventories;
using LexiLink.Modules.Diamond.Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Diamond.Infrastructure.Configuration;

public class DiamondAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public DiamondAutofacModule(string connectionString)
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

        builder.RegisterType<DiamondModule>()
            .As<IDiamondModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<DiamondDomainEventsDispatcher>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<DiamondUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<OutboxAccessor>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(c => new OutboxProcessor(
                connectionString,
                "diamond",
                c.Resolve<IDomainNotificationsMapper>(),
                c.Resolve<IPublisher>(),
                c.ResolveOptional<ILogger<OutboxProcessor>>() ?? NullLogger<OutboxProcessor>.Instance,
                c.Resolve<IClock>(),
                c.ResolveOptional<Microsoft.Extensions.Options.IOptions<OutboxProcessingOptions>>()))
            .As<IOutboxProcessor>()
            .InstancePerLifetimeScope();

        builder.RegisterType<PlayerDiamondInventoryRepository>()
            .As<IPlayerDiamondInventoryRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<DiamondConfigurationService>()
            .As<IDiamondConfigurationService>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

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

        builder.RegisterAssemblyTypes(applicationAssembly)
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(DiamondAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(DiamondAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(IDomainEventNotification<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // AdminAuditing must be innermost so UnitOfWork commits the
        // command mutation and audit outbox row together.
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
