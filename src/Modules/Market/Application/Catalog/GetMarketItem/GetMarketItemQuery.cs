using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Catalog.GetMarketItem;

public sealed class GetMarketItemQuery : QueryBase<MarketItemDto>
{
    public Guid PlayerId { get; }
    public Guid ShopItemId { get; }

    public GetMarketItemQuery(Guid playerId, Guid shopItemId)
    {
        PlayerId = playerId;
        ShopItemId = shopItemId;
    }
}
