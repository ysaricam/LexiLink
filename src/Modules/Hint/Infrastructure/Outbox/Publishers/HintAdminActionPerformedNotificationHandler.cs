using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Hint.Infrastructure.Outbox.DomainEventNotifications;
using MediatR;

namespace LexiLink.Modules.Hint.Infrastructure.Outbox.Publishers;

internal sealed class HintAdminActionPerformedNotificationHandler
    : INotificationHandler<HintAdminActionPerformedNotification>
{
    private readonly IEventsBus _eventsBus;

    internal HintAdminActionPerformedNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        HintAdminActionPerformedNotification notification,
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
