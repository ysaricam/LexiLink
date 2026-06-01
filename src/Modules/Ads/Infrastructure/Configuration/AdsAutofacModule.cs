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
using LexiLink.Modules.Ads.Application.Configuration.Commands;
using LexiLink.Modules.Ads.Application.Configuration.Queries;
using LexiLink.Modules.Ads.Application.Contracts;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using LexiLink.Modules.Ads.Infrastructure.Configuration.Processing;
using LexiLink.Modules.Ads.Infrastructure.Domain;
using LexiLink.Modules.Ads.Infrastructure.Domain.RewardedAdGrants;
using LexiLink.Modules.Ads.Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration;

public class AdsAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public AdsAutofacModule(string connectionString)
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

        builder.RegisterType<AdsModule>()
            .As<IAdsModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<AdsDomainEventsDispatcher>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<AdsUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<OutboxAccessor>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.Register(c => new OutboxProcessor(
                connectionString,
                "ads",
                c.Resolve<IDomainNotificationsMapper>(),
                c.Resolve<IPublisher>(),
                c.ResolveOptional<ILogger<OutboxProcessor>>() ?? NullLogger<OutboxProcessor>.Instance,
                c.Resolve<IClock>(),
                c.ResolveOptional<Microsoft.Extensions.Options.IOptions<OutboxProcessingOptions>>()))
            .As<IOutboxProcessor>()
            .InstancePerLifetimeScope();

        builder.RegisterType<RewardedAdGrantRepository>()
            .As<IRewardedAdGrantRepository>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<AdsConfigurationService>()
            .As<IAdsConfigurationService>()
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

        builder.RegisterAssemblyTypes(typeof(AdsAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterAssemblyTypes(typeof(AdsAutofacModule).Assembly)
            .AsClosedTypesOf(typeof(IDomainEventNotification<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

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
