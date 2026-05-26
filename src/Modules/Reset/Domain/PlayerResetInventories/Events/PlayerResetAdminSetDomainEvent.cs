using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;

public class PlayerResetAdminSetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int NewBalance { get; }
    public DateTime SetOn { get; }

    public PlayerResetAdminSetDomainEvent(Guid playerId, int newBalance, DateTime setOn)
    {
        PlayerId = playerId;
        NewBalance = newBalance;
        SetOn = setOn;
    }
}
