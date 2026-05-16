using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Players.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Players.Infrastructure.Outbox.Publishers;

internal class PlayerRegisteredDomainEventNotificationHandler :
    INotificationHandler<PlayerRegisteredDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal PlayerRegisteredDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        PlayerRegisteredDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new PlayerRegisteredIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.PlayerId,
                notification.DisplayName,
                notification.Discriminator,
                notification.Locale,
                notification.IsGuest),
            cancellationToken);
    }
}
