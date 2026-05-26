using LexiLink.Common.Domain;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

public interface IPlayerUndoInventoryRepository : IRepository<PlayerUndoInventory>
{
    Task<PlayerUndoInventory?> GetByIdAsync(PlayerUndoInventoryId id, CancellationToken cancellationToken = default);

    Task AddAsync(PlayerUndoInventory playerUndoInventory, CancellationToken cancellationToken = default);
}
