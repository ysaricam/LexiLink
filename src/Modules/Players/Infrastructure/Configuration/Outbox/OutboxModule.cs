using Autofac;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;

namespace LexiLink.Modules.Players.Infrastructure.Configuration.Outbox;

public class OutboxModule : Autofac.Module
{
    private readonly BiDictionary<string, Type> _domainNotificationsMap;

    public OutboxModule(BiDictionary<string, Type> domainNotificationsMap)
    {
        _domainNotificationsMap = domainNotificationsMap;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DomainNotificationsMapper>()
            .As<IDomainNotificationsMapper>()
            .WithParameter("domainNotificationMap", _domainNotificationsMap)
            .SingleInstance();
    }

    public static void CheckMappings(BiDictionary<string, Type> domainNotificationsMap)
    {
        var assemblies = new[]
        {
            Assemblies.Application,
            typeof(OutboxModule).Assembly,
        };

        var notificationTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(IDomainEventNotification).IsAssignableFrom(t))
            .ToList();

        var unmapped = notificationTypes
            .Where(t => !domainNotificationsMap.TryGetBySecond(t, out _))
            .Select(t => t.FullName)
            .ToList();

        if (unmapped.Count > 0)
        {
            throw new ApplicationException(
                "Domain notification types are missing from the outbox mapping. " +
                "Add entries in the composition root: " +
                string.Join(", ", unmapped));
        }
    }
}
