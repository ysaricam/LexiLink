using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;

namespace LexiLink.Modules.Quests.Application.Admin.PlayerQuests.ResetPlayerQuest;

public sealed class ResetPlayerQuestCommand : CommandBase, IAdminCommand
{
    public Guid PlayerQuestId { get; }

    public ResetPlayerQuestCommand(Guid playerQuestId)
    {
        PlayerQuestId = playerQuestId;
    }

    public string AuditTargetType => "Quests.PlayerQuest";
    public string? AuditTargetId => PlayerQuestId.ToString();
}
