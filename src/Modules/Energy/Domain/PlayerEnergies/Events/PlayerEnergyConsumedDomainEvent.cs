using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;

public class PlayerEnergyConsumedDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int Amount { get; }
    public int RemainingAmount { get; }

    public PlayerEnergyConsumedDomainEvent(Guid playerId, int amount, int remainingAmount)
    {
        PlayerId = playerId;
        Amount = amount;
        RemainingAmount = remainingAmount;
    }
}
