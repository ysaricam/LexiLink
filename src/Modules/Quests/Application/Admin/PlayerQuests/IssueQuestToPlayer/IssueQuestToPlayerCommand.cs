using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.PlayerQuests.IssueQuestToPlayer;

/// <summary>
/// Admin-side wrap of internal IssueQuestCommand. Same idempotency
/// guarantees (no-op when an active or already-claimed quest exists
/// for the type or when no active definition is found).
/// </summary>
public sealed class IssueQuestToPlayerCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public QuestType QuestType { get; }

    public IssueQuestToPlayerCommand(Guid playerId, QuestType questType)
    {
        PlayerId = playerId;
        QuestType = questType;
    }

    public string AuditTargetType => "Quests.PlayerQuest";
    public string? AuditTargetId => PlayerId.ToString();
}
