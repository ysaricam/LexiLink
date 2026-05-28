using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.PaymentProducts;

internal static class PaymentProductSql
{
    public static async Task<IReadOnlyList<PaymentProductDto>> GetProductsAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        PaymentPlatform? platform,
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();
        const string sql = """
            SELECT
                "Id",
                "StoreProductId",
                "DiamondAmount",
                "IsAppleAvailable",
                "IsGoogleAvailable",
                "SortOrder",
                "IsActive"
            FROM payments."PaymentProducts"
            WHERE (@ActiveOnly = FALSE OR "IsActive" = TRUE)
              AND (
                    @Platform IS NULL
                    OR (@Platform = 1 AND "IsAppleAvailable" = TRUE)
                    OR (@Platform = 2 AND "IsGoogleAvailable" = TRUE)
                  )
            ORDER BY "SortOrder", "DiamondAmount", "StoreProductId";
        """;

        var rows = await connection.QueryAsync<PaymentProductRow>(
            new CommandDefinition(
                sql,
                new
                {
                    ActiveOnly = activeOnly,
                    Platform = platform.HasValue ? (int?)platform.Value : null
                },
                cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    public static async Task<PaymentProductDto> GetProductAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        Guid id,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();
        const string sql = """
            SELECT
                "Id",
                "StoreProductId",
                "DiamondAmount",
                "IsAppleAvailable",
                "IsGoogleAvailable",
                "SortOrder",
                "IsActive"
            FROM payments."PaymentProducts"
            WHERE "Id" = @Id;
        """;

        var row = await connection.QuerySingleOrDefaultAsync<PaymentProductRow>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return row is null
            ? throw new NotFoundException(nameof(PaymentProduct), id)
            : ToDto(row);
    }

    private static PaymentProductDto ToDto(PaymentProductRow row)
    {
        var platforms = new List<PaymentPlatform>(capacity: 2);
        if (row.IsAppleAvailable)
        {
            platforms.Add(PaymentPlatform.Apple);
        }

        if (row.IsGoogleAvailable)
        {
            platforms.Add(PaymentPlatform.Google);
        }

        return new PaymentProductDto(
            row.Id,
            row.StoreProductId,
            row.DiamondAmount,
            row.IsAppleAvailable,
            row.IsGoogleAvailable,
            row.SortOrder,
            row.IsActive,
            platforms);
    }

    private sealed class PaymentProductRow
    {
        public Guid Id { get; init; }
        public string StoreProductId { get; init; } = null!;
        public int DiamondAmount { get; init; }
        public bool IsAppleAvailable { get; init; }
        public bool IsGoogleAvailable { get; init; }
        public int SortOrder { get; init; }
        public bool IsActive { get; init; }
    }
}
