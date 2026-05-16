using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies;

public interface IPlayerEnergyRepository : IRepository<PlayerEnergy>
{
    Task<PlayerEnergy?> GetByIdAsync(PlayerEnergyId id, CancellationToken cancellationToken = default);

    Task AddAsync(PlayerEnergy playerEnergy, CancellationToken cancellationToken = default);
}
