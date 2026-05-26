using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Undo.Infrastructure.Domain.PlayerUndoInventories;

internal class PlayerUndoInventoryRepository : IPlayerUndoInventoryRepository
{
    private readonly UndoContext _undoContext;

    internal PlayerUndoInventoryRepository(UndoContext undoContext)
    {
        _undoContext = undoContext;
    }

    public async Task<PlayerUndoInventory?> GetByIdAsync(PlayerUndoInventoryId id, CancellationToken cancellationToken = default)
    {
        return await _undoContext.PlayerUndoInventories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(PlayerUndoInventory playerUndoInventory, CancellationToken cancellationToken = default)
    {
        await _undoContext.PlayerUndoInventories.AddAsync(playerUndoInventory, cancellationToken);
    }
}
