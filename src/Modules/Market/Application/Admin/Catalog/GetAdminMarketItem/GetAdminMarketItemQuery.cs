using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketItem;

public sealed class GetAdminMarketItemQuery : QueryBase<AdminMarketItemDto>
{
    public Guid ShopItemId { get; }

    public GetAdminMarketItemQuery(Guid shopItemId)
    {
        ShopItemId = shopItemId;
    }
}
