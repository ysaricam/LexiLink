using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain.Events;

public sealed class PaymentNotificationReceivedDomainEvent : DomainEvent
{
    public PaymentNotificationReceivedDomainEvent(
        Guid paymentNotificationId,
        PaymentPlatform platform,
        string notificationId)
    {
        PaymentNotificationId = paymentNotificationId;
        Platform = platform;
        NotificationId = notificationId;
    }

    public Guid PaymentNotificationId { get; }

    public PaymentPlatform Platform { get; }

    public string NotificationId { get; }
}
