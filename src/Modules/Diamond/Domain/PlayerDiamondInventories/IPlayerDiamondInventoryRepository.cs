using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

public interface IPlayerDiamondInventoryRepository : IRepository<PlayerDiamondInventory>
{
    Task<PlayerDiamondInventory?> GetByIdAsync(PlayerDiamondInventoryId id, CancellationToken cancellationToken = default);

    Task AddAsync(PlayerDiamondInventory playerDiamondInventory, CancellationToken cancellationToken = default);
}
