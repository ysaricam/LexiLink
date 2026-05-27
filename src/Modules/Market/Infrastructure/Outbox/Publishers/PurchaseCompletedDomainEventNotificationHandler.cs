using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Market.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Market.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Market.Infrastructure.Outbox.Publishers;

internal sealed class PurchaseCompletedDomainEventNotificationHandler
    : INotificationHandler<PurchaseCompletedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal PurchaseCompletedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        PurchaseCompletedDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new PurchaseCompletedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.PlayerId,
                notification.PurchaseOrderId,
                notification.ShopItemId,
                notification.ItemType,
                notification.Quantity,
                notification.DiamondsPaid,
                notification.PurchasedAt,
                notification.IdempotencyKey),
            cancellationToken);
    }
}
