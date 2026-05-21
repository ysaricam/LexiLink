using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Games.Infrastructure.Outbox.DomainEventNotifications;
using MediatR;

namespace LexiLink.Modules.Games.Infrastructure.Outbox.Publishers;

internal sealed class GamesAdminActionPerformedNotificationHandler
    : INotificationHandler<GamesAdminActionPerformedNotification>
{
    private readonly IEventsBus _eventsBus;

    internal GamesAdminActionPerformedNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        GamesAdminActionPerformedNotification notification,
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
