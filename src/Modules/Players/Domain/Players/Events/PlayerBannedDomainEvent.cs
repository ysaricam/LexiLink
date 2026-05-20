using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class PlayerBannedDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }
    public string Reason { get; }

    public PlayerBannedDomainEvent(PlayerId playerId, string reason)
    {
        PlayerId = playerId;
        Reason = reason;
    }
}
