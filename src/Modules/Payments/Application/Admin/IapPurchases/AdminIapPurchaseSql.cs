using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.IapPurchases;

internal static class AdminIapPurchaseSql
{
    public static async Task<IReadOnlyList<AdminIapPurchaseDto>> GetPurchasesAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        Guid? playerId,
        PaymentPlatform? platform,
        IapPurchaseStatus? status,
        string? storeProductId,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();
        const string sql = """
            SELECT
                "Id",
                "PlayerId",
                "Platform",
                "Environment",
                "StoreProductId",
                "StoreTransactionId",
                "PurchaseToken",
                "OrderId",
                "ClientRequestId",
                "DiamondAmount",
                "Status",
                "PostProcessingAction",
                "PostProcessingStatus",
                "ReceivedAt",
                "VerifiedAt",
                "GrantedAt",
                "FailureReason",
                "PostProcessedAt",
                "PostProcessingFailureReason"
            FROM payments."IapPurchases"
            WHERE (@PlayerId IS NULL OR "PlayerId" = @PlayerId)
              AND (@Platform IS NULL OR "Platform" = @Platform)
              AND (@Status IS NULL OR "Status" = @Status)
              AND (@StoreProductId IS NULL OR "StoreProductId" = @StoreProductId)
            ORDER BY "ReceivedAt" DESC, "Id" DESC
            LIMIT @Limit OFFSET @Offset;
        """;

        var rows = await connection.QueryAsync<IapPurchaseRow>(
            new CommandDefinition(
                sql,
                new
                {
                    PlayerId = playerId,
                    Platform = platform.HasValue ? (int?)platform.Value : null,
                    Status = status.HasValue ? (int?)status.Value : null,
                    StoreProductId = string.IsNullOrWhiteSpace(storeProductId) ? null : storeProductId,
                    Limit = Math.Clamp(limit, 1, 200),
                    Offset = Math.Max(offset, 0)
                },
                cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    public static async Task<AdminIapPurchaseDto> GetPurchaseAsync(
        ISqlConnectionFactory sqlConnectionFactory,
        Guid id,
        CancellationToken cancellationToken)
    {
        var connection = sqlConnectionFactory.GetOpenConnection();
        const string sql = """
            SELECT
                "Id",
                "PlayerId",
                "Platform",
                "Environment",
                "StoreProductId",
                "StoreTransactionId",
                "PurchaseToken",
                "OrderId",
                "ClientRequestId",
                "DiamondAmount",
                "Status",
                "PostProcessingAction",
                "PostProcessingStatus",
                "ReceivedAt",
                "VerifiedAt",
                "GrantedAt",
                "FailureReason",
                "PostProcessedAt",
                "PostProcessingFailureReason"
            FROM payments."IapPurchases"
            WHERE "Id" = @Id;
        """;

        var row = await connection.QuerySingleOrDefaultAsync<IapPurchaseRow>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return row is null
            ? throw new NotFoundException(nameof(IapPurchase), id)
            : ToDto(row);
    }

    private static AdminIapPurchaseDto ToDto(IapPurchaseRow row) =>
        new(
            row.Id,
            row.PlayerId,
            (PaymentPlatform)row.Platform,
            (PaymentEnvironment)row.Environment,
            row.StoreProductId,
            row.StoreTransactionId,
            row.PurchaseToken,
            row.OrderId,
            row.ClientRequestId,
            row.DiamondAmount,
            (IapPurchaseStatus)row.Status,
            (IapPurchasePostProcessingAction)row.PostProcessingAction,
            (IapPurchasePostProcessingStatus)row.PostProcessingStatus,
            row.ReceivedAt,
            row.VerifiedAt,
            row.GrantedAt,
            row.FailureReason,
            row.PostProcessedAt,
            row.PostProcessingFailureReason);

    private sealed class IapPurchaseRow
    {
        public Guid Id { get; init; }
        public Guid PlayerId { get; init; }
        public int Platform { get; init; }
        public int Environment { get; init; }
        public string StoreProductId { get; init; } = null!;
        public string? StoreTransactionId { get; init; }
        public string? PurchaseToken { get; init; }
        public string? OrderId { get; init; }
        public string? ClientRequestId { get; init; }
        public int DiamondAmount { get; init; }
        public int Status { get; init; }
        public int PostProcessingAction { get; init; }
        public int PostProcessingStatus { get; init; }
        public DateTime ReceivedAt { get; init; }
        public DateTime? VerifiedAt { get; init; }
        public DateTime? GrantedAt { get; init; }
        public string? FailureReason { get; init; }
        public DateTime? PostProcessedAt { get; init; }
        public string? PostProcessingFailureReason { get; init; }
    }
}
