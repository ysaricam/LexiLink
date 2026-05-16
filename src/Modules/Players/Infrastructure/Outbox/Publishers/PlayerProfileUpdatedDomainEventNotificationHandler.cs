using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Players.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Players.Infrastructure.Outbox.Publishers;

internal class PlayerProfileUpdatedDomainEventNotificationHandler :
    INotificationHandler<PlayerProfileUpdatedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal PlayerProfileUpdatedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        PlayerProfileUpdatedDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new PlayerProfileUpdatedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.PlayerId,
                notification.AvatarUrl,
                notification.Locale),
            cancellationToken);
    }
}
