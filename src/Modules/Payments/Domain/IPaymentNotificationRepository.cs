namespace LexiLink.Modules.Payments.Domain;

public interface IPaymentNotificationRepository
{
    Task<PaymentNotification?> GetByPlatformAndNotificationIdAsync(
        PaymentPlatform platform,
        string notificationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(PaymentNotification notification, CancellationToken cancellationToken = default);
}
