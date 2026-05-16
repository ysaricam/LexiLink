using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class PlayerQuestClaimedDomainEvent : DomainEvent
{
    public PlayerQuestId PlayerQuestId { get; }
    public Guid PlayerId { get; }
    public QuestType QuestType { get; }
    public int RewardAmount { get; }

    public PlayerQuestClaimedDomainEvent(
        PlayerQuestId playerQuestId,
        Guid playerId,
        QuestType questType,
        int rewardAmount)
    {
        PlayerQuestId = playerQuestId;
        PlayerId = playerId;
        QuestType = questType;
        RewardAmount = rewardAmount;
    }
}
