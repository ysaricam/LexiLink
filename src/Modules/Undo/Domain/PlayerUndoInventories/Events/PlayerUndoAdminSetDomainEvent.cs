using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;

public class PlayerUndoAdminSetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int NewBalance { get; }
    public DateTime SetOn { get; }

    public PlayerUndoAdminSetDomainEvent(Guid playerId, int newBalance, DateTime setOn)
    {
        PlayerId = playerId;
        NewBalance = newBalance;
        SetOn = setOn;
    }
}
