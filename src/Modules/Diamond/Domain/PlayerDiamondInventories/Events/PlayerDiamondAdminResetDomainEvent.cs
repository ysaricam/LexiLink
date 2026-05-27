using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;

public class PlayerDiamondAdminResetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public DateTime ResetOn { get; }

    public PlayerDiamondAdminResetDomainEvent(Guid playerId, DateTime resetOn)
    {
        PlayerId = playerId;
        ResetOn = resetOn;
    }
}
