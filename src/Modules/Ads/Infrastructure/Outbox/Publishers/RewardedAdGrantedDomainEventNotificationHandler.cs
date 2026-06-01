using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Ads.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Ads.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Ads.Infrastructure.Outbox.Publishers;

internal sealed class RewardedAdGrantedDomainEventNotificationHandler
    : INotificationHandler<RewardedAdGrantedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal RewardedAdGrantedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        RewardedAdGrantedDomainEventNotification notification,
        CancellationToken cancellationToken) =>
        _eventsBus.PublishAsync(
            new RewardedAdRewardedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.RewardedAdGrantId,
                notification.PlayerId,
                notification.DiamondAmount,
                notification.TransactionId,
                notification.GrantedOn),
            cancellationToken);
}
