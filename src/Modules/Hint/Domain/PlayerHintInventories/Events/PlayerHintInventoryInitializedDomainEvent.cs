using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;

public class PlayerHintInventoryInitializedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int InitialBalance { get; }

    public PlayerHintInventoryInitializedDomainEvent(Guid playerId, int initialBalance)
    {
        PlayerId = playerId;
        InitialBalance = initialBalance;
    }
}
