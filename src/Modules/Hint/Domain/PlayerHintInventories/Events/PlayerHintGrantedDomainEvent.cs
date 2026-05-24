using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;

public class PlayerHintGrantedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int NewBalance { get; }
    public DateTime GrantedOn { get; }

    public PlayerHintGrantedDomainEvent(Guid playerId, int amount, int newBalance, DateTime grantedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        NewBalance = newBalance;
        GrantedOn = grantedOn;
    }
}
