namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// Read-only view over the active quest catalog. Backed by
/// <see cref="IQuestDefinitionRepository"/> in production; command
/// handlers (e.g. IssueQuestCommand) consult this to find the rules of
/// a given quest type. Returns null when no active definition exists
/// for the type — issuance handlers treat that as a no-op so a
/// deactivated quest stops being issued without breaking active
/// PlayerQuest history.
/// </summary>
public interface IQuestCatalog
{
    Task<QuestDefinition?> ResolveAsync(QuestType questType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuestDefinition>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
