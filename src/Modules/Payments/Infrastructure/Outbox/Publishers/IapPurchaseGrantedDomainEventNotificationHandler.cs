using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Payments.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Payments.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Payments.Infrastructure.Outbox.Publishers;

internal sealed class IapPurchaseGrantedDomainEventNotificationHandler
    : INotificationHandler<IapPurchaseGrantedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal IapPurchaseGrantedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        IapPurchaseGrantedDomainEventNotification notification,
        CancellationToken cancellationToken) =>
        _eventsBus.PublishAsync(
            new IapPurchaseGrantedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.IapPurchaseId,
                notification.PlayerId,
                notification.Platform,
                notification.StoreProductId,
                notification.DiamondAmount,
                notification.GrantedAt),
            cancellationToken);
}
