using LexiLink.Common.Application.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Quests.Infrastructure.Outbox.DomainEventNotifications;

public class PlayerQuestClaimedDomainEventNotification : IDomainEventNotification<PlayerQuestClaimedDomainEvent>
{
    [JsonIgnore]
    public PlayerQuestClaimedDomainEvent DomainEvent { get; private set; } = null!;

    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid PlayerId { get; private set; }
    public Guid PlayerQuestId { get; private set; }
    public string QuestType { get; private set; } = null!;
    public int RewardAmount { get; private set; }

    public PlayerQuestClaimedDomainEventNotification(PlayerQuestClaimedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        PlayerId = domainEvent.PlayerId;
        PlayerQuestId = domainEvent.PlayerQuestId.Value;
        QuestType = domainEvent.QuestType.ToString();
        RewardAmount = domainEvent.RewardAmount;
    }

    [JsonConstructor]
    private PlayerQuestClaimedDomainEventNotification()
    {
    }
}
