using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Payments.Infrastructure.Domain.PaymentNotifications;

internal class PaymentNotificationRepository : IPaymentNotificationRepository
{
    private readonly PaymentsContext _context;

    internal PaymentNotificationRepository(PaymentsContext context)
    {
        _context = context;
    }

    public Task<PaymentNotification?> GetByPlatformAndNotificationIdAsync(
        PaymentPlatform platform,
        string notificationId,
        CancellationToken cancellationToken = default) =>
        _context.PaymentNotifications.FirstOrDefaultAsync(
            x => x.Platform == platform && x.NotificationId == notificationId,
            cancellationToken);

    public async Task AddAsync(PaymentNotification notification, CancellationToken cancellationToken = default) =>
        await _context.PaymentNotifications.AddAsync(notification, cancellationToken);
}
