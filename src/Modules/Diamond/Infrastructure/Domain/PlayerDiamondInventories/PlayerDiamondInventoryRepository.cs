using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Diamond.Infrastructure.Domain.PlayerDiamondInventories;

internal class PlayerDiamondInventoryRepository : IPlayerDiamondInventoryRepository
{
    private readonly DiamondContext _diamondContext;

    internal PlayerDiamondInventoryRepository(DiamondContext diamondContext)
    {
        _diamondContext = diamondContext;
    }

    public async Task<PlayerDiamondInventory?> GetByIdAsync(PlayerDiamondInventoryId id, CancellationToken cancellationToken = default)
    {
        return await _diamondContext.PlayerDiamondInventories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(PlayerDiamondInventory playerDiamondInventory, CancellationToken cancellationToken = default)
    {
        await _diamondContext.PlayerDiamondInventories.AddAsync(playerDiamondInventory, cancellationToken);
    }
}
