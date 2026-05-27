using Autofac;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Market.Infrastructure.Configuration.Outbox;
using LexiLink.Modules.Market.Infrastructure.Outbox.DomainEventNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Market.Infrastructure.Configuration;

public static class MarketStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    static MarketStartup()
    {
        DomainNotificationsMap.Add(
            "Market.PurchaseCompletedDomainEventNotification",
            typeof(PurchaseCompletedDomainEventNotification));
        DomainNotificationsMap.Add(
            "Market.AdminActionPerformedNotification",
            typeof(MarketAdminActionPerformedNotification));
    }

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MarketContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new MarketAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
