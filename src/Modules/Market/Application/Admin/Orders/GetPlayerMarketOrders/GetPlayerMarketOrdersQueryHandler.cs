using LexiLink.Common.Application.Data;
using LexiLink.Modules.Market.Application.Configuration.Queries;
using LexiLink.Modules.Market.Application.Orders;

namespace LexiLink.Modules.Market.Application.Admin.Orders.GetPlayerMarketOrders;

internal sealed class GetPlayerMarketOrdersQueryHandler
    : IQueryHandler<GetPlayerMarketOrdersQuery, IReadOnlyList<MarketOrderDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerMarketOrdersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<IReadOnlyList<MarketOrderDto>> Handle(
        GetPlayerMarketOrdersQuery request,
        CancellationToken cancellationToken) =>
        MarketOrdersSql.GetByPlayerAsync(
            _sqlConnectionFactory,
            request.PlayerId,
            request.Limit,
            request.Offset,
            cancellationToken);
}
