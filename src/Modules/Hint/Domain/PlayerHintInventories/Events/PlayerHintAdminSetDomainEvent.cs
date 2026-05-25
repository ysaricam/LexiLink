using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;

public class PlayerHintAdminSetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int NewBalance { get; }
    public DateTime SetOn { get; }

    public PlayerHintAdminSetDomainEvent(Guid playerId, int newBalance, DateTime setOn)
    {
        PlayerId = playerId;
        NewBalance = newBalance;
        SetOn = setOn;
    }
}
