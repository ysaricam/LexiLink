using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.ConsumePlayerEnergy;

public class ConsumePlayerEnergyCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public ConsumePlayerEnergyCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
