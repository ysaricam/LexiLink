using Autofac;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Payments.Infrastructure.Configuration.Outbox;
using LexiLink.Modules.Payments.Infrastructure.Outbox.DomainEventNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Payments.Infrastructure.Configuration;

public static class PaymentsStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    static PaymentsStartup()
    {
        DomainNotificationsMap.Add(
            "Payments.AdminActionPerformedNotification",
            typeof(PaymentsAdminActionPerformedNotification));
        DomainNotificationsMap.Add(
            "Payments.IapPurchaseGranted",
            typeof(IapPurchaseGrantedDomainEventNotification));
        DomainNotificationsMap.Add(
            "Payments.IapPurchaseStatusChanged",
            typeof(IapPurchaseStatusChangedDomainEventNotification));
    }

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaymentsContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new PaymentsAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
