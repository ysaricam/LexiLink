using Autofac;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Stats.Application.Configuration.Commands;
using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Application.Configuration.Queries;
using LexiLink.Modules.Stats.Application.Contracts;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;
using LexiLink.Modules.Stats.Infrastructure.Inbox;
using LexiLink.Modules.Stats.Infrastructure.InternalCommands;
using LexiLink.Modules.Stats.Infrastructure.Queries;

namespace LexiLink.Modules.Stats.Infrastructure.Configuration;

public class StatsAutofacModule : Autofac.Module
{
    private readonly string _connectionString;

    public StatsAutofacModule(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void Load(ContainerBuilder builder)
    {
        var allCtors = new AllConstructorFinder();
        var applicationAssembly = Assemblies.Application;

        builder.Register(_ => new SqlConnectionFactory(_connectionString))
            .As<ISqlConnectionFactory>()
            .InstancePerLifetimeScope();

        builder.RegisterType<StatsModule>()
            .As<IStatsModule>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<PlayerStatsProjectionUpdater>()
            .As<IPlayerStatsProjectionUpdater>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<StatsInbox>()
            .As<IStatsInbox>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<StatsInboxProcessor>()
            .As<IStatsInboxProcessor>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<StatsInternalCommandScheduler>()
            .As<IStatsInternalCommandScheduler>()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);

        builder.RegisterType<StatsInternalCommandProcessor>()
            .As<IStatsInternalCommandProcessor>()
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
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope()
            .FindConstructorsWith(allCtors);
    }
}
