using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;

public class PlayerEnergyRefilledDomainEvent : DomainEvent
{
    public Guid PlayerId { get; }
    public int GainedAmount { get; }
    public int CurrentAmount { get; }

    public PlayerEnergyRefilledDomainEvent(Guid playerId, int gainedAmount, int currentAmount)
    {
        PlayerId = playerId;
        GainedAmount = gainedAmount;
        CurrentAmount = currentAmount;
    }
}
