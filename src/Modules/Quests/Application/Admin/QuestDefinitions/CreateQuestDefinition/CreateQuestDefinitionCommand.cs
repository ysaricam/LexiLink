using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;

public sealed class CreateQuestDefinitionCommand : CommandBase<Guid>, IAdminCommand
{
    public string Name { get; }
    public string Description { get; }
    public QuestTrigger Trigger { get; }
    public int Threshold { get; }
    public int Reward { get; }
    public Guid? PrerequisiteQuestDefinitionId { get; }
    public ProgressBaseline ProgressBaseline { get; }

    public CreateQuestDefinitionCommand(
        string name,
        string description,
        QuestTrigger trigger,
        int threshold,
        int reward,
        Guid? prerequisiteQuestDefinitionId,
        ProgressBaseline progressBaseline)
    {
        Name = name;
        Description = description;
        Trigger = trigger;
        Threshold = threshold;
        Reward = reward;
        PrerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        ProgressBaseline = progressBaseline;
    }

    public string AuditTargetType => "Quests.QuestDefinition";

    // Id is allocated inside the handler — see PayloadJson on the audit
    // row for the resulting QuestDefinitionId.
    public string? AuditTargetId => null;
}
