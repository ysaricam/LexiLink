using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Undo.Infrastructure.Outbox.DomainEventNotifications;
using MediatR;

namespace LexiLink.Modules.Undo.Infrastructure.Outbox.Publishers;

internal sealed class UndoAdminActionPerformedNotificationHandler
    : INotificationHandler<UndoAdminActionPerformedNotification>
{
    private readonly IEventsBus _eventsBus;

    internal UndoAdminActionPerformedNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        UndoAdminActionPerformedNotification notification,
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
