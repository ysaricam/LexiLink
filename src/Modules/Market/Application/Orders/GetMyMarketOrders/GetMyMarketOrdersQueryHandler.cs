using LexiLink.Common.Application.Data;
using LexiLink.Modules.Market.Application.Configuration.Queries;

namespace LexiLink.Modules.Market.Application.Orders.GetMyMarketOrders;

internal sealed class GetMyMarketOrdersQueryHandler
    : IQueryHandler<GetMyMarketOrdersQuery, IReadOnlyList<MarketOrderDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetMyMarketOrdersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<IReadOnlyList<MarketOrderDto>> Handle(
        GetMyMarketOrdersQuery request,
        CancellationToken cancellationToken) =>
        MarketOrdersSql.GetByPlayerAsync(
            _sqlConnectionFactory,
            request.PlayerId,
            request.Limit,
            request.Offset,
            cancellationToken);
}
