using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Purchases.BuyShopItem;

public sealed class BuyShopItemCommand : CommandBase<BuyShopItemResultDto>
{
    public Guid PlayerId { get; }
    public Guid ShopItemId { get; }
    public string IdempotencyKey { get; }

    public BuyShopItemCommand(Guid playerId, Guid shopItemId, string idempotencyKey)
    {
        PlayerId = playerId;
        ShopItemId = shopItemId;
        IdempotencyKey = idempotencyKey;
    }
}
