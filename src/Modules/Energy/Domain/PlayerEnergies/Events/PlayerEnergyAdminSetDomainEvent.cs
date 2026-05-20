using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;

public class PlayerEnergyAdminSetDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int NewAmount { get; }
    public int MaximumAmount { get; }

    public PlayerEnergyAdminSetDomainEvent(Guid playerId, int newAmount, int maximumAmount)
    {
        PlayerId = playerId;
        NewAmount = newAmount;
        MaximumAmount = maximumAmount;
    }
}
