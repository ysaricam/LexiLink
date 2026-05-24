using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories;

public interface IPlayerHintInventoryRepository : IRepository<PlayerHintInventory>
{
    Task<PlayerHintInventory?> GetByIdAsync(PlayerHintInventoryId id, CancellationToken cancellationToken = default);

    Task AddAsync(PlayerHintInventory playerHintInventory, CancellationToken cancellationToken = default);
}
