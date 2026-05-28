namespace LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;

public sealed record PaymentNotificationResultDto(
    Guid NotificationId,
    string Platform,
    string NotificationType,
    string Status,
    Guid? IapPurchaseId,
    string? IapPurchaseStatus,
    bool IsReplay,
    string? FailureReason);
