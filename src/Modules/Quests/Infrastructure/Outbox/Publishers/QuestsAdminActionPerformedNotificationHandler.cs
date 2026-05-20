using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Quests.Infrastructure.Outbox.DomainEventNotifications;
using MediatR;

namespace LexiLink.Modules.Quests.Infrastructure.Outbox.Publishers;

internal sealed class QuestsAdminActionPerformedNotificationHandler
    : INotificationHandler<QuestsAdminActionPerformedNotification>
{
    private readonly IEventsBus _eventsBus;

    internal QuestsAdminActionPerformedNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        QuestsAdminActionPerformedNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
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
}
