using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Players.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Players.Infrastructure.Outbox.Publishers;

internal class AuthProviderLinkedDomainEventNotificationHandler :
    INotificationHandler<AuthProviderLinkedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal AuthProviderLinkedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        AuthProviderLinkedDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new AuthProviderLinkedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.PlayerId,
                notification.Provider,
                notification.ExternalId),
            cancellationToken);
    }
}
