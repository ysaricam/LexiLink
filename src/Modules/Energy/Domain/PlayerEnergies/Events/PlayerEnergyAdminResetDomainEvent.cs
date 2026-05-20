using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;

public class PlayerEnergyAdminResetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int MaximumAmount { get; }

    public PlayerEnergyAdminResetDomainEvent(Guid playerId, int maximumAmount)
    {
        PlayerId = playerId;
        MaximumAmount = maximumAmount;
    }
}
