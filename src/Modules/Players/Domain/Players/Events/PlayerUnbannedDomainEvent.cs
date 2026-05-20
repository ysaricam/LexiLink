using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class PlayerUnbannedDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }

    public PlayerUnbannedDomainEvent(PlayerId playerId)
    {
        PlayerId = playerId;
    }
}
