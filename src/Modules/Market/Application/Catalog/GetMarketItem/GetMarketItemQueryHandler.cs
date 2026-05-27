using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Market.Application.Configuration.Queries;

namespace LexiLink.Modules.Market.Application.Catalog.GetMarketItem;

internal sealed class GetMarketItemQueryHandler : IQueryHandler<GetMarketItemQuery, MarketItemDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetMarketItemQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public Task<MarketItemDto> Handle(GetMarketItemQuery request, CancellationToken cancellationToken) =>
        MarketCatalogSql.GetVisibleItemAsync(
            _sqlConnectionFactory,
            _clock,
            request.PlayerId,
            request.ShopItemId,
            cancellationToken);
}
