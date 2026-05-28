using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Payments.Infrastructure.Outbox.DomainEventNotifications;
using MediatR;

namespace LexiLink.Modules.Payments.Infrastructure.Outbox.Publishers;

internal sealed class PaymentsAdminActionPerformedNotificationHandler
    : INotificationHandler<PaymentsAdminActionPerformedNotification>
{
    private readonly IEventsBus _eventsBus;

    internal PaymentsAdminActionPerformedNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        PaymentsAdminActionPerformedNotification notification,
        CancellationToken cancellationToken) =>
        _eventsBus.PublishAsync(
            new AdminActionPerformedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.AdminUserId,
                notification.ActionType,
                notification.TargetType,
                notification.TargetId,
                notification.PayloadJson),
            cancellationToken);
}
