using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Market.Infrastructure.Domain.ShopItems;

internal class ShopItemRepository : IShopItemRepository
{
    private readonly MarketContext _marketContext;

    internal ShopItemRepository(MarketContext marketContext)
    {
        _marketContext = marketContext;
    }

    public async Task<ShopItem?> GetByIdAsync(ShopItemId id, CancellationToken cancellationToken = default)
    {
        return await _marketContext.ShopItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ShopItem shopItem, CancellationToken cancellationToken = default)
    {
        await _marketContext.ShopItems.AddAsync(shopItem, cancellationToken);
    }
}
