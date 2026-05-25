using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class QuestDefinitionUpdatedDomainEvent : DomainEvent
{
    public Guid QuestDefinitionId { get; }
    public string Description { get; }
    public int Threshold { get; }
    public int EnergyReward { get; }
    public int HintReward { get; }
    public Guid? PrerequisiteQuestDefinitionId { get; }
    public string ProgressBaseline { get; }

    public QuestDefinitionUpdatedDomainEvent(
        Guid questDefinitionId,
        string description,
        int threshold,
        int energyReward,
        int hintReward,
        Guid? prerequisiteQuestDefinitionId,
        string progressBaseline)
    {
        QuestDefinitionId = questDefinitionId;
        Description = description;
        Threshold = threshold;
        EnergyReward = energyReward;
        HintReward = hintReward;
        PrerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        ProgressBaseline = progressBaseline;
    }
}
