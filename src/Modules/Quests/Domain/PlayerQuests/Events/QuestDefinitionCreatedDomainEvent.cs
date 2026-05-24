using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class QuestDefinitionCreatedDomainEvent : DomainEvent
{
    public Guid QuestDefinitionId { get; }
    public string Name { get; }
    public string Trigger { get; }
    public int Threshold { get; }
    public int Reward { get; }
    public Guid? PrerequisiteQuestDefinitionId { get; }
    public string ProgressBaseline { get; }

    public QuestDefinitionCreatedDomainEvent(
        Guid questDefinitionId,
        string name,
        string trigger,
        int threshold,
        int reward,
        Guid? prerequisiteQuestDefinitionId,
        string progressBaseline)
    {
        QuestDefinitionId = questDefinitionId;
        Name = name;
        Trigger = trigger;
        Threshold = threshold;
        Reward = reward;
        PrerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        ProgressBaseline = progressBaseline;
    }
}
