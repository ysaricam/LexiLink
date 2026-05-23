using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.ReactivateQuestDefinition;

public sealed class ReactivateQuestDefinitionCommand : CommandBase, IAdminCommand
{
    public Guid QuestDefinitionId { get; }

    public ReactivateQuestDefinitionCommand(Guid questDefinitionId)
    {
        QuestDefinitionId = questDefinitionId;
    }

    public string AuditTargetType => "Quests.QuestDefinition";
    public string? AuditTargetId => QuestDefinitionId.ToString();
}
