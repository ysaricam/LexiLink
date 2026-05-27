using Autofac;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Diamond.Infrastructure.Configuration.Outbox;
using LexiLink.Modules.Diamond.Infrastructure.Outbox.DomainEventNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Diamond.Infrastructure.Configuration;

public static class DiamondStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    static DiamondStartup()
    {
        DomainNotificationsMap.Add(
            "Diamond.AdminActionPerformedNotification",
            typeof(DiamondAdminActionPerformedNotification));
    }

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DiamondContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new DiamondAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
