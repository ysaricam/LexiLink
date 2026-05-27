using LexiLink.Modules.Market.Application.Contracts;
using LexiLink.Modules.Market.Application.Orders;

namespace LexiLink.Modules.Market.Application.Admin.Orders.GetPlayerMarketOrders;

public sealed class GetPlayerMarketOrdersQuery : QueryBase<IReadOnlyList<MarketOrderDto>>
{
    public Guid PlayerId { get; }
    public int Limit { get; }
    public int Offset { get; }

    public GetPlayerMarketOrdersQuery(Guid playerId, int limit = 50, int offset = 0)
    {
        PlayerId = playerId;
        Limit = Math.Clamp(limit, 1, 100);
        Offset = Math.Max(offset, 0);
    }
}
