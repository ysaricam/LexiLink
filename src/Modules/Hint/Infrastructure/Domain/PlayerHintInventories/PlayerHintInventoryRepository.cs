using LexiLink.Modules.Hint.Domain.PlayerHintInventories;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Hint.Infrastructure.Domain.PlayerHintInventories;

internal class PlayerHintInventoryRepository : IPlayerHintInventoryRepository
{
    private readonly HintContext _hintContext;

    internal PlayerHintInventoryRepository(HintContext hintContext)
    {
        _hintContext = hintContext;
    }

    public async Task<PlayerHintInventory?> GetByIdAsync(PlayerHintInventoryId id, CancellationToken cancellationToken = default)
    {
        return await _hintContext.PlayerHintInventories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(PlayerHintInventory playerHintInventory, CancellationToken cancellationToken = default)
    {
        await _hintContext.PlayerHintInventories.AddAsync(playerHintInventory, cancellationToken);
    }
}
