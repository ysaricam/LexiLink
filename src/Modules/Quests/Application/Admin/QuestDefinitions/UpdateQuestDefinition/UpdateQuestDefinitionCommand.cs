using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;

public sealed class UpdateQuestDefinitionCommand : CommandBase, IAdminCommand
{
    public Guid QuestDefinitionId { get; }
    public string Description { get; }
    public int Threshold { get; }
    public int EnergyReward { get; }
    public int HintReward { get; }
    public int UndoReward { get; }
    public int ResetReward { get; }
    public Guid? PrerequisiteQuestDefinitionId { get; }
    public ProgressBaseline ProgressBaseline { get; }

    public UpdateQuestDefinitionCommand(
        Guid questDefinitionId,
        string description,
        int threshold,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward,
        Guid? prerequisiteQuestDefinitionId,
        ProgressBaseline progressBaseline)
    {
        QuestDefinitionId = questDefinitionId;
        Description = description;
        Threshold = threshold;
        EnergyReward = energyReward;
        HintReward = hintReward;
        UndoReward = undoReward;
        ResetReward = resetReward;
        PrerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        ProgressBaseline = progressBaseline;
    }

    public string AuditTargetType => "Quests.QuestDefinition";
    public string? AuditTargetId => QuestDefinitionId.ToString();
}
