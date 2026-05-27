using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.Catalog;

internal static class AdminMarketCatalogSql
{
    public static async Task<IReadOnlyList<AdminMarketCategoryDto>> GetCategoriesAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();
        const string sql = """
            SELECT
                "Id",
                "Name",
                "SortOrder",
                "Icon",
                "IsActive",
                "VisibilityStartsAt",
                "VisibilityEndsAt"
            FROM "market"."Categories"
            ORDER BY "SortOrder", "Name";
        """;

        var rows = await connection.QueryAsync<AdminMarketCategoryDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public static async Task<IReadOnlyList<AdminMarketItemDto>> GetItemsAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        IClock clock,
        Guid? categoryId,
        ItemType? itemType,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var rows = await QueryItemsAsync(
            sqlConnectionFactory,
            clock,
            shopItemId: null,
            categoryId,
            itemType,
            isActive,
            cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    public static async Task<AdminMarketItemDto> GetItemAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        IClock clock,
        Guid shopItemId,
        CancellationToken cancellationToken)
    {
        var row = (await QueryItemsAsync(
                sqlConnectionFactory,
                clock,
                shopItemId,
                categoryId: null,
                itemType: null,
                isActive: null,
                cancellationToken))
            .SingleOrDefault();

        return row is null
            ? throw new NotFoundException(nameof(ShopItem), shopItemId)
            : ToDto(row);
    }

    private static async Task<IEnumerable<AdminMarketItemRow>> QueryItemsAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        IClock clock,
        Guid? shopItemId,
        Guid? categoryId,
        ItemType? itemType,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();
        const string sql = """
            SELECT
                si."Id"                   AS "Id",
                si."CategoryId"           AS "CategoryId",
                c."Name"                  AS "CategoryName",
                si."ItemType"             AS "ItemType",
                si."Quantity"             AS "Quantity",
                si."Price"                AS "Price",
                CASE
                    WHEN si."PromoPrice" IS NOT NULL
                         AND si."PromotionStartsAt" <= @Now
                         AND @Now < si."PromotionEndsAt"
                    THEN si."PromoPrice"
                    ELSE si."Price"
                END                       AS "EffectivePrice",
                si."PromoPrice"           AS "PromoPrice",
                si."PromotionStartsAt"    AS "PromotionStartsAt",
                si."PromotionEndsAt"      AS "PromotionEndsAt",
                si."MaxStock"             AS "MaxStock",
                si."SoldCount"            AS "SoldCount",
                CASE
                    WHEN si."MaxStock" IS NULL THEN NULL
                    ELSE GREATEST(si."MaxStock" - si."SoldCount", 0)
                END                       AS "RemainingStock",
                si."PerPlayerLimit"       AS "PerPlayerLimit",
                si."PerPlayerLimitWindow" AS "PerPlayerLimitWindow",
                si."IsActive"             AS "IsActive",
                si."Version"              AS "Version"
            FROM "market"."ShopItems" si
            INNER JOIN "market"."Categories" c ON c."Id" = si."CategoryId"
            WHERE (@ShopItemId IS NULL OR si."Id" = @ShopItemId)
              AND (@CategoryId IS NULL OR si."CategoryId" = @CategoryId)
              AND (@ItemType IS NULL OR si."ItemType" = @ItemType)
              AND (@IsActive IS NULL OR si."IsActive" = @IsActive)
            ORDER BY c."SortOrder", c."Name", si."Id";
        """;

        return await connection.QueryAsync<AdminMarketItemRow>(
            new CommandDefinition(
                sql,
                new
                {
                    Now = clock.UtcNow,
                    ShopItemId = shopItemId,
                    CategoryId = categoryId,
                    ItemType = itemType.HasValue ? (int?)itemType.Value : null,
                    IsActive = isActive
                },
                cancellationToken: cancellationToken));
    }

    private static AdminMarketItemDto ToDto(AdminMarketItemRow row) =>
        new(
            row.Id,
            row.CategoryId,
            row.CategoryName,
            (ItemType)row.ItemType,
            row.Quantity,
            row.Price,
            row.EffectivePrice,
            row.PromoPrice,
            row.PromotionStartsAt,
            row.PromotionEndsAt,
            row.MaxStock,
            row.SoldCount,
            row.RemainingStock,
            row.PerPlayerLimit,
            (PerPlayerLimitWindow)row.PerPlayerLimitWindow,
            row.IsActive,
            row.Version);

    private sealed class AdminMarketItemRow
    {
        public Guid Id { get; init; }
        public Guid CategoryId { get; init; }
        public string CategoryName { get; init; } = null!;
        public int ItemType { get; init; }
        public int Quantity { get; init; }
        public int Price { get; init; }
        public int EffectivePrice { get; init; }
        public int? PromoPrice { get; init; }
        public DateTime? PromotionStartsAt { get; init; }
        public DateTime? PromotionEndsAt { get; init; }
        public int? MaxStock { get; init; }
        public int SoldCount { get; init; }
        public int? RemainingStock { get; init; }
        public int? PerPlayerLimit { get; init; }
        public int PerPlayerLimitWindow { get; init; }
        public bool IsActive { get; init; }
        public uint Version { get; init; }
    }
}
