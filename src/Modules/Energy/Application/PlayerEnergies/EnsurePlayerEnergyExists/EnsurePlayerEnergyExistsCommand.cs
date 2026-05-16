using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.PlayerEnergies.EnsurePlayerEnergyExists;

public class EnsurePlayerEnergyExistsCommand : CommandBase
{
    public Guid PlayerId { get; }

    public EnsurePlayerEnergyExistsCommand(Guid playerId)
    {
        PlayerId = playerId;
    }
}
