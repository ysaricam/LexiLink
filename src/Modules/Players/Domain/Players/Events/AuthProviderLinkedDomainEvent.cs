using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class AuthProviderLinkedDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }
    public AuthProvider Provider { get; }
    public string ExternalId { get; }

    public AuthProviderLinkedDomainEvent(PlayerId playerId, AuthProvider provider, string externalId)
    {
        PlayerId = playerId;
        Provider = provider;
        ExternalId = externalId;
    }
}
