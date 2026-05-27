using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;

public class PlayerDiamondGrantedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int NewBalance { get; }
    public DateTime GrantedOn { get; }

    public PlayerDiamondGrantedDomainEvent(Guid playerId, int amount, int newBalance, DateTime grantedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        NewBalance = newBalance;
        GrantedOn = grantedOn;
    }
}
