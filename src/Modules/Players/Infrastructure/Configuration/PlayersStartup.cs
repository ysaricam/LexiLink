using Autofac;
using LexiLink.Common.Application;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Players.Infrastructure.Configuration.Outbox;
using LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Players.Infrastructure.Configuration;

public static class PlayersStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    static PlayersStartup()
    {
        DomainNotificationsMap.Add(
            "Players.PlayerRegisteredDomainEventNotification",
            typeof(PlayerRegisteredDomainEventNotification));
        DomainNotificationsMap.Add(
            "Players.AuthProviderLinkedDomainEventNotification",
            typeof(AuthProviderLinkedDomainEventNotification));
        DomainNotificationsMap.Add(
            "Players.PlayerProfileUpdatedDomainEventNotification",
            typeof(PlayerProfileUpdatedDomainEventNotification));
        DomainNotificationsMap.Add(
            "Players.AdminActionPerformedNotification",
            typeof(PlayersAdminActionPerformedNotification));
    }

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PlayersContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new PlayersAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
