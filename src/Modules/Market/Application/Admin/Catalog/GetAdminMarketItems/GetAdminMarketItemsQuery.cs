using LexiLink.Modules.Market.Application.Contracts;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketItems;

public sealed class GetAdminMarketItemsQuery : QueryBase<IReadOnlyList<AdminMarketItemDto>>
{
    public Guid? CategoryId { get; }
    public ItemType? ItemType { get; }
    public bool? IsActive { get; }

    public GetAdminMarketItemsQuery(Guid? categoryId, ItemType? itemType, bool? isActive)
    {
        CategoryId = categoryId;
        ItemType = itemType;
        IsActive = isActive;
    }
}
