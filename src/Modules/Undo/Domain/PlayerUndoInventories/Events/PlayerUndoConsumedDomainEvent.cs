using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;

public class PlayerUndoConsumedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int RemainingBalance { get; }
    public DateTime ConsumedOn { get; }

    public PlayerUndoConsumedDomainEvent(Guid playerId, int amount, int remainingBalance, DateTime consumedOn)
    {
        PlayerId = playerId;
        Amount = amount;
        RemainingBalance = remainingBalance;
        ConsumedOn = consumedOn;
    }
}
