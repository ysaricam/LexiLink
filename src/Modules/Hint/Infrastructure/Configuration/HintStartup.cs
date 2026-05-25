using Autofac;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Hint.Infrastructure.Configuration.Outbox;
using LexiLink.Modules.Hint.Infrastructure.Outbox.DomainEventNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Hint.Infrastructure.Configuration;

public static class HintStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    static HintStartup()
    {
        DomainNotificationsMap.Add(
            "Hint.AdminActionPerformedNotification",
            typeof(HintAdminActionPerformedNotification));
    }

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HintContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new HintAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
