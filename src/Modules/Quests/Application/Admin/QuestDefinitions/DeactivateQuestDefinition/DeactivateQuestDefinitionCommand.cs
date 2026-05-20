using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.DeactivateQuestDefinition;

public sealed class DeactivateQuestDefinitionCommand : CommandBase, IAdminCommand
{
    public Guid QuestDefinitionId { get; }

    public DeactivateQuestDefinitionCommand(Guid questDefinitionId)
    {
        QuestDefinitionId = questDefinitionId;
    }

    public string AuditTargetType => "Quests.QuestDefinition";
    public string? AuditTargetId => QuestDefinitionId.ToString();
}
