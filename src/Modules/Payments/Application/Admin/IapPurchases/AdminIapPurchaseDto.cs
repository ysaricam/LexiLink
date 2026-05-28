using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.IapPurchases;

public sealed record AdminIapPurchaseDto(
    Guid Id,
    Guid PlayerId,
    PaymentPlatform Platform,
    PaymentEnvironment Environment,
    string StoreProductId,
    string? StoreTransactionId,
    string? PurchaseToken,
    string? OrderId,
    string? ClientRequestId,
    int DiamondAmount,
    IapPurchaseStatus Status,
    IapPurchasePostProcessingAction PostProcessingAction,
    IapPurchasePostProcessingStatus PostProcessingStatus,
    DateTime ReceivedAt,
    DateTime? VerifiedAt,
    DateTime? GrantedAt,
    string? FailureReason,
    DateTime? PostProcessedAt,
    string? PostProcessingFailureReason);
