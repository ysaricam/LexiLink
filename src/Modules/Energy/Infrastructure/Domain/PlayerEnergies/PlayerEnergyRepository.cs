using LexiLink.Modules.Energy.Domain.PlayerEnergies;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Energy.Infrastructure.Domain.PlayerEnergies;

internal class PlayerEnergyRepository : IPlayerEnergyRepository
{
    private readonly EnergyContext _energyContext;

    internal PlayerEnergyRepository(EnergyContext energyContext)
    {
        _energyContext = energyContext;
    }

    public async Task<PlayerEnergy?> GetByIdAsync(PlayerEnergyId id, CancellationToken cancellationToken = default)
    {
        return await _energyContext.PlayerEnergies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(PlayerEnergy playerEnergy, CancellationToken cancellationToken = default)
    {
        await _energyContext.PlayerEnergies.AddAsync(playerEnergy, cancellationToken);
    }
}
