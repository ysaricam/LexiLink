using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Domain.Events;
using LexiLink.Modules.Payments.Domain.Rules;

namespace LexiLink.Modules.Payments.Domain;

public class PaymentNotification : Entity, IAggregateRoot
{
    private const int NotificationIdMaxLength = 256;
    private const int NotificationTypeMaxLength = 128;
    private const int FailureReasonMaxLength = 1000;

    public PaymentNotificationId Id { get; private set; }
    private PaymentPlatform _platform;
    private PaymentEnvironment _environment;
    private string _notificationId = null!;
    private string _notificationType = null!;
    private string _payloadJson = null!;
    private DateTime _receivedAt;
    private DateTime? _processedAt;
    private PaymentNotificationStatus _status;
    private string? _failureReason;

    public PaymentPlatform Platform => _platform;
    public PaymentEnvironment Environment => _environment;
    public string NotificationId => _notificationId;
    public string NotificationType => _notificationType;
    public string PayloadJson => _payloadJson;
    public DateTime ReceivedAt => _receivedAt;
    public DateTime? ProcessedAt => _processedAt;
    public PaymentNotificationStatus Status => _status;
    public string? FailureReason => _failureReason;

    private PaymentNotification()
    {
        Id = null!;
    }

    private PaymentNotification(
        PaymentNotificationId id,
        PaymentPlatform platform,
        PaymentEnvironment environment,
        string notificationId,
        string notificationType,
        string payloadJson,
        DateTime receivedAt)
    {
        Id = id;
        _platform = platform;
        _environment = environment;
        _notificationId = notificationId.Trim();
        _notificationType = notificationType.Trim();
        _payloadJson = payloadJson.Trim();
        _receivedAt = receivedAt;
        _status = PaymentNotificationStatus.Received;

        AddDomainEvent(new PaymentNotificationReceivedDomainEvent(
            Id.Value,
            _platform,
            _notificationId));
    }

    internal static PaymentNotification Receive(
        PaymentPlatform platform,
        PaymentEnvironment environment,
        string notificationId,
        string notificationType,
        string payloadJson,
        DateTime receivedAt)
    {
        CheckRule(new TextMustNotBeEmptyRule(notificationId, nameof(notificationId)));
        CheckRule(new TextMustNotExceedMaxLengthRule(notificationId.Trim(), NotificationIdMaxLength, nameof(notificationId)));
        CheckRule(new TextMustNotBeEmptyRule(notificationType, nameof(notificationType)));
        CheckRule(new TextMustNotExceedMaxLengthRule(notificationType.Trim(), NotificationTypeMaxLength, nameof(notificationType)));
        CheckRule(new TextMustNotBeEmptyRule(payloadJson, nameof(payloadJson)));

        return new PaymentNotification(
            new PaymentNotificationId(Guid.NewGuid()),
            platform,
            environment,
            notificationId,
            notificationType,
            payloadJson,
            receivedAt);
    }

    internal void MarkProcessed(DateTime processedAt)
    {
        _status = PaymentNotificationStatus.Processed;
        _processedAt = processedAt;
        _failureReason = null;
    }

    internal void MarkFailed(string failureReason)
    {
        var normalized = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();
        CheckRule(new TextMustNotBeEmptyRule(normalized, nameof(failureReason)));
        CheckRule(new TextMustNotExceedMaxLengthRule(normalized, FailureReasonMaxLength, nameof(failureReason)));

        _status = PaymentNotificationStatus.Failed;
        _failureReason = normalized;
    }
}
