using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Payments.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Payments.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Payments.Infrastructure.Outbox.Publishers;

internal sealed class IapPurchaseStatusChangedDomainEventNotificationHandler
    : INotificationHandler<IapPurchaseStatusChangedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal IapPurchaseStatusChangedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        IapPurchaseStatusChangedDomainEventNotification notification,
        CancellationToken cancellationToken) =>
        _eventsBus.PublishAsync(
            new IapPurchaseStatusChangedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.IapPurchaseId,
                notification.Status),
            cancellationToken);
}
