using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Quests.Infrastructure.Outbox.DomainEventNotifications;
using LexiLink.Modules.Quests.IntegrationEvents;
using MediatR;

namespace LexiLink.Modules.Quests.Infrastructure.Outbox.Publishers;

internal class PlayerQuestClaimedDomainEventNotificationHandler :
    INotificationHandler<PlayerQuestClaimedDomainEventNotification>
{
    private readonly IEventsBus _eventsBus;

    internal PlayerQuestClaimedDomainEventNotificationHandler(IEventsBus eventsBus)
    {
        _eventsBus = eventsBus;
    }

    public Task Handle(
        PlayerQuestClaimedDomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        return _eventsBus.PublishAsync(
            new QuestClaimedIntegrationEvent(
                notification.Id,
                notification.OccurredOn,
                notification.PlayerId,
                notification.PlayerQuestId,
                notification.QuestDefinitionId,
                notification.EnergyReward,
                notification.HintReward),
            cancellationToken);
    }
}
