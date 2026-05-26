using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Reset.Infrastructure.Outbox.DomainEventNotifications;
using MediatR;

namespace LexiLink.Modules.Reset.Infrastructure.Outbox.Publishers;

internal sealed class ResetAdminActionPerformedNotificationHandler
    : INotificationHandler<ResetAdminActionPerformedNotification>
{
    private readonly IEventsBus _eventsBus;

    internal ResetAdminActionPerformedNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        ResetAdminActionPerformedNotification notification,
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
