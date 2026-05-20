using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class QuestDefinitionUpdatedDomainEvent : DomainEvent
{
    public Guid QuestDefinitionId { get; }
    public int Goal { get; }
    public int RewardAmount { get; }
    public string? PrerequisiteQuestType { get; }

    public QuestDefinitionUpdatedDomainEvent(
        Guid questDefinitionId,
        int goal,
        int rewardAmount,
        string? prerequisiteQuestType)
    {
        QuestDefinitionId = questDefinitionId;
        Goal = goal;
        RewardAmount = rewardAmount;
        PrerequisiteQuestType = prerequisiteQuestType;
    }
}
