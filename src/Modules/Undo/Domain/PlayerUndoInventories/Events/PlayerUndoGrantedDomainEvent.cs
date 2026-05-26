using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;

public class PlayerUndoGrantedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int NewBalance { get; }
    public DateTime GrantedOn { get; }

    public PlayerUndoGrantedDomainEvent(Guid playerId, int amount, int newBalance, DateTime grantedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        NewBalance = newBalance;
        GrantedOn = grantedOn;
    }
}
