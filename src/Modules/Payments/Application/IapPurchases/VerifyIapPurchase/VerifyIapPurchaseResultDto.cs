namespace LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;

public sealed record VerifyIapPurchaseResultDto(
    Guid PaymentId,
    string ProductId,
    int DiamondAmount,
    string Status,
    string PostProcessingAction,
    string PostProcessingStatus,
    bool CanFinishTransaction,
    string? PostProcessingFailureReason,
    bool IsReplay);
