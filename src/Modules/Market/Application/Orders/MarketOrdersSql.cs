using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Orders;

internal static class MarketOrdersSql
{
    public static async Task<IReadOnlyList<MarketOrderDto>> GetByPlayerAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        Guid playerId,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Id"             AS "Id",
                "PlayerId"       AS "PlayerId",
                "ShopItemId"     AS "ShopItemId",
                "ItemType"       AS "ItemType",
                "Quantity"       AS "Quantity",
                "DiamondsPaid"   AS "DiamondsPaid",
                "PurchasedAt"    AS "PurchasedAt",
                "IdempotencyKey" AS "IdempotencyKey"
            FROM "market"."PurchaseOrders"
            WHERE "PlayerId" = @PlayerId
            ORDER BY "PurchasedAt" DESC, "Id" DESC
            LIMIT @Limit OFFSET @Offset;
        """;

        var rows = await connection.QueryAsync<OrderRow>(
            new CommandDefinition(
                sql,
                new { PlayerId = playerId, Limit = limit, Offset = offset },
                cancellationToken: cancellationToken));

        return rows
            .Select(row => new MarketOrderDto(
                row.Id,
                row.PlayerId,
                row.ShopItemId,
                (ItemType)row.ItemType,
                row.Quantity,
                row.DiamondsPaid,
                row.PurchasedAt,
                row.IdempotencyKey))
            .ToList();
    }

    private sealed class OrderRow
    {
        public Guid Id { get; init; }
        public Guid PlayerId { get; init; }
        public Guid ShopItemId { get; init; }
        public int ItemType { get; init; }
        public int Quantity { get; init; }
        public int DiamondsPaid { get; init; }
        public DateTime PurchasedAt { get; init; }
        public string IdempotencyKey { get; init; } = null!;
    }
}
