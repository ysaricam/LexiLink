using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;

public sealed class CreateQuestDefinitionCommand : CommandBase<Guid>, IAdminCommand
{
    public QuestType QuestType { get; }
    public QuestCadence Cadence { get; }
    public int Goal { get; }
    public int RewardAmount { get; }
    public QuestType? PrerequisiteQuestType { get; }

    public CreateQuestDefinitionCommand(
        QuestType questType,
        QuestCadence cadence,
        int goal,
        int rewardAmount,
        QuestType? prerequisiteQuestType)
    {
        QuestType = questType;
        Cadence = cadence;
        Goal = goal;
        RewardAmount = rewardAmount;
        PrerequisiteQuestType = prerequisiteQuestType;
    }

    public string AuditTargetType => "Quests.QuestDefinition";

    // Id is allocated inside the handler — see PayloadJson on the audit
    // row for the resulting QuestDefinitionId.
    public string? AuditTargetId => null;
}
