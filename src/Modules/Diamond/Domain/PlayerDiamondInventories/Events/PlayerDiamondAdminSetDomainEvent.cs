using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;

public class PlayerDiamondAdminSetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int NewBalance { get; }
    public DateTime SetOn { get; }

    public PlayerDiamondAdminSetDomainEvent(Guid playerId, int newBalance, DateTime setOn)
    {
        PlayerId = playerId;
        NewBalance = newBalance;
        SetOn = setOn;
    }
}
