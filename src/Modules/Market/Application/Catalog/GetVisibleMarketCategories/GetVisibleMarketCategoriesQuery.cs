using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Catalog.GetVisibleMarketCategories;

public sealed class GetVisibleMarketCategoriesQuery : QueryBase<IReadOnlyList<MarketCategoryDto>>
{
    public Guid PlayerId { get; }

    public GetVisibleMarketCategoriesQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
