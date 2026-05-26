using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;

public class PlayerResetInventoryInitializedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int InitialBalance { get; }

    public PlayerResetInventoryInitializedDomainEvent(Guid playerId, int initialBalance)
    {
        PlayerId = playerId;
        InitialBalance = initialBalance;
    }
}
