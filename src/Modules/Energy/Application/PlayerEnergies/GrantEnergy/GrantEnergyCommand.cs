using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

public class GrantEnergyCommand : CommandBase
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantEnergyCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
