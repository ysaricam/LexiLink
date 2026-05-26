using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;

public class PlayerResetAdminResetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public DateTime ResetOn { get; }

    public PlayerResetAdminResetDomainEvent(Guid playerId, DateTime resetOn)
    {
        PlayerId = playerId;
        ResetOn = resetOn;
    }
}
