using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Orders.GetMyMarketOrders;

public sealed class GetMyMarketOrdersQuery : QueryBase<IReadOnlyList<MarketOrderDto>>
{
    public Guid PlayerId { get; }
    public int Limit { get; }
    public int Offset { get; }

    public GetMyMarketOrdersQuery(Guid playerId, int limit = 50, int offset = 0)
    {
        PlayerId = playerId;
        Limit = Math.Clamp(limit, 1, 100);
        Offset = Math.Max(offset, 0);
    }
}
