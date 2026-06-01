using Autofac;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Ads.Infrastructure.Configuration.Outbox;
using LexiLink.Modules.Ads.Infrastructure.Outbox.DomainEventNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration;

public static class AdsStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    static AdsStartup()
    {
        DomainNotificationsMap.Add(
            "Ads.RewardedAdGrantedNotification",
            typeof(RewardedAdGrantedDomainEventNotification));
    }

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AdsContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new AdsAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
