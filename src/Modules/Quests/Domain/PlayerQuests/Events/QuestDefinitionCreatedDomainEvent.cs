using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

public class QuestDefinitionCreatedDomainEvent : DomainEvent
{
    public Guid QuestDefinitionId { get; }
    public string Name { get; }
    public string Trigger { get; }
    public int Threshold { get; }
    public int EnergyReward { get; }
    public int HintReward { get; }
    public int UndoReward { get; }
    public int ResetReward { get; }
    public Guid? PrerequisiteQuestDefinitionId { get; }
    public string ProgressBaseline { get; }

    public QuestDefinitionCreatedDomainEvent(
        Guid questDefinitionId,
        string name,
        string trigger,
        int threshold,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward,
        Guid? prerequisiteQuestDefinitionId,
        string progressBaseline)
    {
        QuestDefinitionId = questDefinitionId;
        Name = name;
        Trigger = trigger;
        Threshold = threshold;
        EnergyReward = energyReward;
        HintReward = hintReward;
        UndoReward = undoReward;
        ResetReward = resetReward;
        PrerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        ProgressBaseline = progressBaseline;
    }
}
