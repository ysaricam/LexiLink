using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Configuration.Verification;

public sealed record AppleIapVerificationRequest(
    Guid PlayerId,
    string StoreProductId,
    string TransactionId,
    string? SignedTransactionJws,
    string? AppAccountToken);

public sealed record GooglePlayIapVerificationRequest(
    Guid PlayerId,
    string StoreProductId,
    string PurchaseToken,
    string? ObfuscatedAccountId,
    string? ObfuscatedProfileId);

public sealed record StorePurchaseVerificationResult(
    bool IsVerified,
    PaymentPlatform Platform,
    PaymentEnvironment Environment,
    string StoreProductId,
    string? StoreTransactionId,
    string? PurchaseToken,
    string? OrderId,
    string? AccountToken,
    StorePurchaseState PurchaseState,
    StorePurchasePostProcessingAction PostProcessingAction,
    DateTime? PurchasedAt,
    string? FailureReason)
{
    public static StorePurchaseVerificationResult Verified(
        PaymentPlatform platform,
        PaymentEnvironment environment,
        string storeProductId,
        string? storeTransactionId,
        string? purchaseToken,
        string? orderId,
        string? accountToken,
        StorePurchasePostProcessingAction postProcessingAction,
        DateTime? purchasedAt) =>
        new(
            true,
            platform,
            environment,
            storeProductId,
            storeTransactionId,
            purchaseToken,
            orderId,
            accountToken,
            StorePurchaseState.Purchased,
            postProcessingAction,
            purchasedAt,
            FailureReason: null);

    public static StorePurchaseVerificationResult Failed(
        PaymentPlatform platform,
        PaymentEnvironment environment,
        string storeProductId,
        string? storeTransactionId,
        string? purchaseToken,
        StorePurchaseState purchaseState,
        string failureReason) =>
        new(
            false,
            platform,
            environment,
            storeProductId,
            storeTransactionId,
            purchaseToken,
            OrderId: null,
            AccountToken: null,
            purchaseState,
            StorePurchasePostProcessingAction.None,
            PurchasedAt: null,
            failureReason);
}

public enum StorePurchaseState
{
    Unknown = 0,
    Purchased = 1,
    Pending = 2,
    Cancelled = 3,
    Refunded = 4,
    Revoked = 5,
    ProductMismatch = 6,
    AccountMismatch = 7,
    Invalid = 8
}

public enum StorePurchasePostProcessingAction
{
    None = 0,
    AppleClientFinishTransaction = 1,
    GoogleAcknowledge = 2,
    GoogleConsume = 3
}
