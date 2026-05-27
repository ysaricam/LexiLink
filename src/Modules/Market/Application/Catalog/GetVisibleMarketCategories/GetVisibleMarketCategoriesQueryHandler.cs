using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Market.Application.Configuration.Queries;

namespace LexiLink.Modules.Market.Application.Catalog.GetVisibleMarketCategories;

internal sealed class GetVisibleMarketCategoriesQueryHandler
    : IQueryHandler<GetVisibleMarketCategoriesQuery, IReadOnlyList<MarketCategoryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetVisibleMarketCategoriesQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public Task<IReadOnlyList<MarketCategoryDto>> Handle(
        GetVisibleMarketCategoriesQuery request,
        CancellationToken cancellationToken) =>
        MarketCatalogSql.GetVisibleCategoriesAsync(
            _sqlConnectionFactory,
            _clock,
            request.PlayerId,
            cancellationToken);
}
