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
using LexiLink.Modules.Undo.Application.Configuration.Commands;
using LexiLink.Modules.Undo.Application.Configuration.Queries;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using LexiLink.Modules.Undo.Infrastructure.Configuration.Processing;
using LexiLink.Modules.Undo.Infrastructure.Domain;
using LexiLink.Modules.Undo.Infrastructure.Domain.PlayerUndoInventories;
using LexiLink.Modules.Undo.Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Undo.Infrastructure.Configuration;

public class UndoAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public UndoAutofacModule(string connectionString)
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

        builder.RegisterType<UndoModule>()
            .As<IUndoModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<UndoDomainEventsDispatcher>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<UndoUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<OutboxAccessor>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(c => new OutboxProcessor(
                connectionString,
                "undo",
                c.Resolve<IDomainNotificationsMapper>(),
                c.Resolve<IPublisher>(),
                c.ResolveOptional<ILogger<OutboxProcessor>>() ?? NullLogger<OutboxProcessor>.Instance,
                c.Resolve<IClock>(),
                c.ResolveOptional<Microsoft.Extensions.Options.IOptions<OutboxProcessingOptions>>()))
            .As<IOutboxProcessor>()
            .InstancePerLifetimeScope();

        builder.RegisterType<PlayerUndoInventoryRepository>()
            .As<IPlayerUndoInventoryRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<UndoConfigurationService>()
            .As<IUndoConfigurationService>()
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

        builder.RegisterAssemblyTypes(typeof(UndoAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(UndoAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(IDomainEventNotification<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        // AdminAuditing must be the INNERMOST decorator (registered first
        // = wrapped first = innermost). UnitOfWork wraps it so the outbox
        // row enqueued for IAdminCommand commits in the same SaveChangesAsync
        // as the command's domain changes. Mirrors the Energy/Hint pattern.
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
