using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;

public class PlayerResetGrantedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int NewBalance { get; }
    public DateTime GrantedOn { get; }

    public PlayerResetGrantedDomainEvent(Guid playerId, int amount, int newBalance, DateTime grantedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        NewBalance = newBalance;
        GrantedOn = grantedOn;
    }
}
