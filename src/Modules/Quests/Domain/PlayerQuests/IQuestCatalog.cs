namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// Read-only view over the active quest catalog. Backed by
/// <see cref="IQuestDefinitionRepository"/> in production; command
/// handlers (e.g. IssueQuestCommand, ClaimQuestCommandHandler) consult
/// this to find the rules of a given definition. Returns null when the
/// definition exists but is inactive, so issuance handlers can no-op
/// without breaking PlayerQuest history.
/// </summary>
public interface IQuestCatalog
{
    Task<QuestDefinition?> ResolveAsync(QuestDefinitionId questDefinitionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuestDefinition>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
