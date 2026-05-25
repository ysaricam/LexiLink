using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;

public class PlayerHintAdminResetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public DateTime ResetOn { get; }

    public PlayerHintAdminResetDomainEvent(Guid playerId, DateTime resetOn)
    {
        PlayerId = playerId;
        ResetOn = resetOn;
    }
}
