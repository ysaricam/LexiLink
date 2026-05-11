using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class PlayerProfileUpdatedDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }

    public PlayerProfileUpdatedDomainEvent(PlayerId playerId)
    {
        PlayerId = playerId;
    }
}
