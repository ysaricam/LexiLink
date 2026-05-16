using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Games.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Games.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Games.Infrastructure.Outbox.Publishers;

internal class GameCompletedDomainEventNotificationHandler :
    INotificationHandler<GameCompletedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal GameCompletedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        GameCompletedDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new GameCompletedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.GameId,
                notification.PlayerId,
                notification.StartLinkId,
                notification.TargetLinkId,
                notification.Score),
            cancellationToken);
    }
}
