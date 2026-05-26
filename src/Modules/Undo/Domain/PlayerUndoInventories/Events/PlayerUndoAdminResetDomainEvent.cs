using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;

public class PlayerUndoAdminResetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public DateTime ResetOn { get; }

    public PlayerUndoAdminResetDomainEvent(Guid playerId, DateTime resetOn)
    {
        PlayerId = playerId;
        ResetOn = resetOn;
    }
}
