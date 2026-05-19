using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Administration.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Administration.Infrastructure.Outbox.Publishers;

internal class AdminUserRegisteredDomainEventNotificationHandler :
    INotificationHandler<AdminUserRegisteredDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal AdminUserRegisteredDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        AdminUserRegisteredDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new AdminUserRegisteredIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.AdminUserId,
                notification.Email,
                notification.Role),
            cancellationToken);
    }
}
