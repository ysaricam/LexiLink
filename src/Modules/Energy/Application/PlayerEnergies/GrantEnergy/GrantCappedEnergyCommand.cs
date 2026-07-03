using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

public sealed class GrantCappedEnergyCommand : CommandBase<int>
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantCappedEnergyCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }
}
