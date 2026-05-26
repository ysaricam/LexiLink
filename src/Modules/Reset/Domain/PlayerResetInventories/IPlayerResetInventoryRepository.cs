using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories;

public interface IPlayerResetInventoryRepository : IRepository<PlayerResetInventory>
{
    Task<PlayerResetInventory?> GetByIdAsync(PlayerResetInventoryId id, CancellationToken cancellationToken = default);

    Task AddAsync(PlayerResetInventory playerResetInventory, CancellationToken cancellationToken = default);
}
