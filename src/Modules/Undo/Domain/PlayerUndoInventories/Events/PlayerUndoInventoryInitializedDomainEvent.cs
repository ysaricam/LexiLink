using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;

public class PlayerUndoInventoryInitializedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int InitialBalance { get; }

    public PlayerUndoInventoryInitializedDomainEvent(Guid playerId, int initialBalance)
    {
        PlayerId = playerId;
        InitialBalance = initialBalance;
    }
}
