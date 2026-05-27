using LexiLink.Modules.Energy.Application.Configuration.CrossModule;
using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GrantEnergy;

namespace LexiLink.API.CrossModule;

// API-host adapter for Market -> Energy grants.
internal class EnergyGrant : IEnergyGrant
{
    private readonly IEnergyModule _energyModule;

    public EnergyGrant(IEnergyModule energyModule)
    {
        _energyModule = energyModule;
    }

    public Task GrantAsync(
        Guid playerId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        return _energyModule.ExecuteCommandAsync(
            new GrantEnergyCommand(playerId, amount),
            cancellationToken);
    }
}
