using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Catalog;

internal static class MarketCatalogSql
{
    public static async Task<IReadOnlyList<MarketCategoryDto>> GetVisibleCategoriesAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        IClock clock,
        Guid playerId,
        CancellationToken cancellationToken)
    {
        var rows = (await QueryVisibleRowsAsync(
            sqlConnectionFactory,
            clock,
            playerId,
            shopItemId: null,
            cancellationToken)).ToList();

        return rows
            .GroupBy(row => new
            {
                row.CategoryId,
                row.CategoryName,
                row.SortOrder,
                row.Icon,
                row.CategoryIsActive,
                row.VisibilityStartsAt,
                row.VisibilityEndsAt
            })
            .Select(group => new MarketCategoryDto(
                group.Key.CategoryId,
                group.Key.CategoryName,
                group.Key.SortOrder,
                group.Key.Icon,
                group.Key.CategoryIsActive,
                group.Key.VisibilityStartsAt,
                group.Key.VisibilityEndsAt,
                group
                    .Where(row => row.ShopItemId.HasValue)
                    .Select(ToItemDto)
                    .ToList()))
            .ToList();
    }

    public static async Task<MarketItemDto> GetVisibleItemAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        IClock clock,
        Guid playerId,
        Guid shopItemId,
        CancellationToken cancellationToken)
    {
        var row = (await QueryVisibleRowsAsync(
                sqlConnectionFactory,
                clock,
                playerId,
                shopItemId,
                cancellationToken))
            .SingleOrDefault(x => x.ShopItemId.HasValue);

        return row is null
            ? throw new NotFoundException(nameof(ShopItem), shopItemId)
            : ToItemDto(row);
    }

    private static async Task<IEnumerable<VisibleMarketRow>> QueryVisibleRowsAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        IClock clock,
        Guid playerId,
        Guid? shopItemId,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var connection = sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                c."Id"                 AS "CategoryId",
                c."Name"               AS "CategoryName",
                c."SortOrder"          AS "SortOrder",
                c."Icon"               AS "Icon",
                c."IsActive"           AS "CategoryIsActive",
                c."VisibilityStartsAt" AS "VisibilityStartsAt",
                c."VisibilityEndsAt"   AS "VisibilityEndsAt",
                si."Id"                AS "ShopItemId",
                si."ItemType"          AS "ItemType",
                si."Quantity"          AS "Quantity",
                si."Price"             AS "Price",
                CASE
                    WHEN si."PromoPrice" IS NOT NULL
                         AND si."PromotionStartsAt" <= @Now
                         AND @Now < si."PromotionEndsAt"
                    THEN si."PromoPrice"
                    ELSE si."Price"
                END                    AS "EffectivePrice",
                si."PromoPrice"        AS "PromoPrice",
                si."PromotionStartsAt" AS "PromotionStartsAt",
                si."PromotionEndsAt"   AS "PromotionEndsAt",
                si."MaxStock"          AS "MaxStock",
                si."SoldCount"         AS "SoldCount",
                CASE
                    WHEN si."MaxStock" IS NULL THEN NULL
                    ELSE GREATEST(si."MaxStock" - si."SoldCount", 0)
                END                    AS "RemainingStock",
                si."PerPlayerLimit"       AS "PerPlayerLimit",
                si."PerPlayerLimitWindow" AS "PerPlayerLimitWindow",
                CASE
                    WHEN si."PerPlayerLimit" IS NULL THEN NULL
                    ELSE GREATEST(si."PerPlayerLimit" - (
                        SELECT COUNT(*)
                        FROM "market"."PurchaseOrders" po
                        WHERE po."PlayerId" = @PlayerId
                          AND po."ShopItemId" = si."Id"
                          AND (
                              si."PerPlayerLimitWindow" = 0
                              OR (si."PerPlayerLimitWindow" = 1
                                  AND po."PurchasedAt" >= @Today
                                  AND po."PurchasedAt" < @Tomorrow)
                              OR (si."PerPlayerLimitWindow" = 2
                                  AND (
                                      si."PromotionStartsAt" IS NULL
                                      OR NOT (si."PromotionStartsAt" <= @Now AND @Now < si."PromotionEndsAt")
                                      OR (po."PurchasedAt" >= si."PromotionStartsAt"
                                          AND po."PurchasedAt" < si."PromotionEndsAt")
                                  ))
                          )
                    ), 0)
                END                    AS "PerPlayerRemaining",
                si."IsActive"          AS "ShopItemIsActive"
            FROM "market"."Categories" c
            LEFT JOIN "market"."ShopItems" si
                ON si."CategoryId" = c."Id"
               AND si."IsActive" = TRUE
               AND (@ShopItemId IS NULL OR si."Id" = @ShopItemId)
            WHERE c."IsActive" = TRUE
              AND (c."VisibilityStartsAt" IS NULL
                   OR (c."VisibilityStartsAt" <= @Now AND @Now < c."VisibilityEndsAt"))
              AND (@ShopItemId IS NULL OR si."Id" = @ShopItemId)
            ORDER BY c."SortOrder", c."Name", si."Id";
        """;

        return await connection.QueryAsync<VisibleMarketRow>(
            new CommandDefinition(
                sql,
                new { PlayerId = playerId, Now = now, Today = today, Tomorrow = tomorrow, ShopItemId = shopItemId },
                cancellationToken: cancellationToken));
    }

    private static MarketItemDto ToItemDto(VisibleMarketRow row) =>
        new(
            row.ShopItemId!.Value,
            row.CategoryId,
            (ItemType)row.ItemType!.Value,
            row.Quantity!.Value,
            row.Price!.Value,
            row.EffectivePrice!.Value,
            row.PromoPrice,
            row.PromotionStartsAt,
            row.PromotionEndsAt,
            row.MaxStock,
            row.SoldCount!.Value,
            row.RemainingStock,
            row.PerPlayerLimit,
            (PerPlayerLimitWindow)row.PerPlayerLimitWindow!.Value,
            row.PerPlayerRemaining,
            row.ShopItemIsActive!.Value);

    private sealed class VisibleMarketRow
    {
        public Guid CategoryId { get; init; }
        public string CategoryName { get; init; } = null!;
        public int SortOrder { get; init; }
        public string? Icon { get; init; }
        public bool CategoryIsActive { get; init; }
        public DateTime? VisibilityStartsAt { get; init; }
        public DateTime? VisibilityEndsAt { get; init; }
        public Guid? ShopItemId { get; init; }
        public int? ItemType { get; init; }
        public int? Quantity { get; init; }
        public int? Price { get; init; }
        public int? EffectivePrice { get; init; }
        public int? PromoPrice { get; init; }
        public DateTime? PromotionStartsAt { get; init; }
        public DateTime? PromotionEndsAt { get; init; }
        public int? MaxStock { get; init; }
        public int? SoldCount { get; init; }
        public int? RemainingStock { get; init; }
        public int? PerPlayerLimit { get; init; }
        public int? PerPlayerLimitWindow { get; init; }
        public int? PerPlayerRemaining { get; init; }
        public bool? ShopItemIsActive { get; init; }
    }
}
