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
    public Guid QuestDefinitionId { get; private set; }
    public int EnergyReward { get; private set; }
    public int HintReward { get; private set; }
    public int UndoReward { get; private set; }
    public int ResetReward { get; private set; }
    public int DiamondReward { get; private set; }

    public PlayerQuestClaimedDomainEventNotification(PlayerQuestClaimedDomainEvent domainEvent, Guid id)
    {
        DomainEvent = domainEvent;
        Id = id;
        OccurredOn = domainEvent.OccurredOn;
        PlayerId = domainEvent.PlayerId;
        PlayerQuestId = domainEvent.PlayerQuestId.Value;
        QuestDefinitionId = domainEvent.QuestDefinitionId.Value;
        EnergyReward = domainEvent.EnergyReward;
        HintReward = domainEvent.HintReward;
        UndoReward = domainEvent.UndoReward;
        ResetReward = domainEvent.ResetReward;
        DiamondReward = domainEvent.DiamondReward;
    }

    [JsonConstructor]
    private PlayerQuestClaimedDomainEventNotification()
    {
    }
}
