using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Stats.Infrastructure.Inbox;

internal static class StatsInboxMessageTypeMap
{
    private static readonly IReadOnlyDictionary<string, Type> NameToType = new Dictionary<string, Type>
    {
        [typeof(PlayerRegisteredIntegrationEvent).FullName!] = typeof(PlayerRegisteredIntegrationEvent),
        [typeof(AuthProviderLinkedIntegrationEvent).FullName!] = typeof(AuthProviderLinkedIntegrationEvent),
        [typeof(PlayerProfileUpdatedIntegrationEvent).FullName!] = typeof(PlayerProfileUpdatedIntegrationEvent),
        [typeof(GameCompletedIntegrationEvent).FullName!] = typeof(GameCompletedIntegrationEvent)
    };

    public static string GetName(IIntegrationEvent integrationEvent) =>
        integrationEvent.GetType().FullName
        ?? throw new InvalidOperationException($"Integration event type '{integrationEvent.GetType()}' has no full name.");

    public static Type? GetType(string typeName) =>
        NameToType.GetValueOrDefault(typeName);
}
