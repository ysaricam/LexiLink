using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;

public sealed class UpdateQuestDefinitionCommand : CommandBase, IAdminCommand
{
    public Guid QuestDefinitionId { get; }
    public int Goal { get; }
    public int RewardAmount { get; }
    public QuestType? PrerequisiteQuestType { get; }

    public UpdateQuestDefinitionCommand(
        Guid questDefinitionId,
        int goal,
        int rewardAmount,
        QuestType? prerequisiteQuestType)
    {
        QuestDefinitionId = questDefinitionId;
        Goal = goal;
        RewardAmount = rewardAmount;
        PrerequisiteQuestType = prerequisiteQuestType;
    }

    public string AuditTargetType => "Quests.QuestDefinition";
    public string? AuditTargetId => QuestDefinitionId.ToString();
}
