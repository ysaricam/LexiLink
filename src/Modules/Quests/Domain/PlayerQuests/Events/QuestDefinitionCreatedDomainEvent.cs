using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class QuestDefinitionCreatedDomainEvent : DomainEvent
{
    public Guid QuestDefinitionId { get; }
    public string QuestType { get; }
    public string Cadence { get; }
    public int Goal { get; }
    public int RewardAmount { get; }
    public string? PrerequisiteQuestType { get; }

    public QuestDefinitionCreatedDomainEvent(
        Guid questDefinitionId,
        string questType,
        string cadence,
        int goal,
        int rewardAmount,
        string? prerequisiteQuestType)
    {
        QuestDefinitionId = questDefinitionId;
        QuestType = questType;
        Cadence = cadence;
        Goal = goal;
        RewardAmount = rewardAmount;
        PrerequisiteQuestType = prerequisiteQuestType;
    }
}
