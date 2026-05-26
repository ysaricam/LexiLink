using LexiLink.Modules.Reset.Domain.PlayerResetInventories;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Reset.Infrastructure.Domain.PlayerResetInventories;

internal class PlayerResetInventoryRepository : IPlayerResetInventoryRepository
{
    private readonly ResetContext _resetContext;

    internal PlayerResetInventoryRepository(ResetContext resetContext)
    {
        _resetContext = resetContext;
    }

    public async Task<PlayerResetInventory?> GetByIdAsync(PlayerResetInventoryId id, CancellationToken cancellationToken = default)
    {
        return await _resetContext.PlayerResetInventories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(PlayerResetInventory playerResetInventory, CancellationToken cancellationToken = default)
    {
        await _resetContext.PlayerResetInventories.AddAsync(playerResetInventory, cancellationToken);
    }
}
