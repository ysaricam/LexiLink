using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;

public class PlayerDiamondInventoryInitializedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int InitialBalance { get; }

    public PlayerDiamondInventoryInitializedDomainEvent(Guid playerId, int initialBalance)
    {
        PlayerId = playerId;
        InitialBalance = initialBalance;
    }
}
