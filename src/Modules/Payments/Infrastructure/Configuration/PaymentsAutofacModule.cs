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
using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Application.Configuration.Notifications;
using LexiLink.Modules.Payments.Application.Configuration.Queries;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Domain;
using LexiLink.Modules.Payments.Infrastructure.Configuration.Processing;
using LexiLink.Modules.Payments.Infrastructure.Domain;
using LexiLink.Modules.Payments.Infrastructure.Domain.IapPurchases;
using LexiLink.Modules.Payments.Infrastructure.Domain.PaymentNotifications;
using LexiLink.Modules.Payments.Infrastructure.Domain.PaymentProducts;
using LexiLink.Modules.Payments.Infrastructure.Notifications;
using LexiLink.Modules.Payments.Infrastructure.Outbox;
using LexiLink.Modules.Payments.Infrastructure.Verification.Apple;
using LexiLink.Modules.Payments.Infrastructure.Verification.Google;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Payments.Infrastructure.Configuration;

public class PaymentsAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public PaymentsAutofacModule(string connectionString)
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

        builder.RegisterType<PaymentsModule>()
            .As<IPaymentsModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<PaymentsDomainEventsDispatcher>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<PaymentsUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<OutboxAccessor>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(c => new OutboxProcessor(
                connectionString,
                "payments",
                c.Resolve<IDomainNotificationsMapper>(),
                c.Resolve<IPublisher>(),
                c.ResolveOptional<ILogger<OutboxProcessor>>() ?? NullLogger<OutboxProcessor>.Instance,
                c.Resolve<IClock>(),
                c.ResolveOptional<Microsoft.Extensions.Options.IOptions<OutboxProcessingOptions>>()))
            .As<IOutboxProcessor>()
            .InstancePerLifetimeScope();

        builder.RegisterType<PaymentProductRepository>()
            .As<IPaymentProductRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<IapPurchaseRepository>()
            .As<IIapPurchaseRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<PaymentNotificationRepository>()
            .As<IPaymentNotificationRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<AppleIapVerifier>()
            .As<IAppleIapVerifier>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<GooglePlayIapVerifier>()
            .As<IGooglePlayIapVerifier>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<GooglePlayPurchaseProcessor>()
            .As<IGooglePlayPurchaseProcessor>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<AppleServerNotificationVerifier>()
            .As<IAppleServerNotificationVerifier>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<GoogleRealTimeDeveloperNotificationVerifier>()
            .As<IGoogleRealTimeDeveloperNotificationVerifier>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(applicationAssembly)
            .Where(x => x.Name.EndsWith("Processor", StringComparison.Ordinal))
            .AsSelf()
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

        builder.RegisterAssemblyTypes(typeof(PaymentsAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(PaymentsAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(IDomainEventNotification<>))
            .AsImplementedInterfaces()
            .InstancePerDependency()
            .FindConstructorsWith(allCtors);

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
