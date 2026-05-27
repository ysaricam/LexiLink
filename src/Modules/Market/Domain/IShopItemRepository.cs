namespace LexiLink.Modules.Market.Domain;

public interface IShopItemRepository
{
    Task<ShopItem?> GetByIdAsync(ShopItemId id, CancellationToken cancellationToken = default);

    Task AddAsync(ShopItem shopItem, CancellationToken cancellationToken = default);
}
