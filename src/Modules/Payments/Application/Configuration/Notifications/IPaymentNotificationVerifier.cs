using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Configuration.Notifications;

public interface IAppleServerNotificationVerifier
{
    Task<PaymentNotificationVerificationResult> VerifyAsync(
        AppleServerNotificationVerificationRequest request,
        CancellationToken cancellationToken = default);
}

public interface IGoogleRealTimeDeveloperNotificationVerifier
{
    Task<PaymentNotificationVerificationResult> VerifyAsync(
        GoogleRealTimeDeveloperNotificationVerificationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AppleServerNotificationVerificationRequest(string SignedPayload);

public sealed record GoogleRealTimeDeveloperNotificationVerificationRequest(
    string MessageId,
    string PayloadJson,
    string? AuthorizationToken);

public sealed record PaymentNotificationVerificationResult(
    bool IsVerified,
    PaymentPlatform Platform,
    PaymentEnvironment Environment,
    string NotificationId,
    string NotificationType,
    string PayloadJson,
    string? StoreTransactionId,
    string? PurchaseToken,
    PaymentNotificationPurchaseStatus PurchaseStatus,
    string? Reason,
    string? FailureReason)
{
    public static PaymentNotificationVerificationResult Verified(
        PaymentPlatform platform,
        PaymentEnvironment environment,
        string notificationId,
        string notificationType,
        string payloadJson,
        string? storeTransactionId,
        string? purchaseToken,
        PaymentNotificationPurchaseStatus purchaseStatus,
        string? reason = null) =>
        new(
            true,
            platform,
            environment,
            notificationId,
            notificationType,
            payloadJson,
            storeTransactionId,
            purchaseToken,
            purchaseStatus,
            reason,
            FailureReason: null);

    public static PaymentNotificationVerificationResult Failed(
        PaymentPlatform platform,
        PaymentEnvironment environment,
        string notificationId,
        string notificationType,
        string payloadJson,
        string failureReason) =>
        new(
            false,
            platform,
            environment,
            notificationId,
            notificationType,
            payloadJson,
            StoreTransactionId: null,
            PurchaseToken: null,
            PaymentNotificationPurchaseStatus.None,
            Reason: null,
            failureReason);
}

public enum PaymentNotificationPurchaseStatus
{
    None = 0,
    Refunded = 1,
    Revoked = 2,
    Failed = 3
}
