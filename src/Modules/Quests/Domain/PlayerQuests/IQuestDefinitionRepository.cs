using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

public interface IQuestDefinitionRepository : IRepository<QuestDefinition>
{
    Task<QuestDefinition?> GetByIdAsync(QuestDefinitionId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuestDefinition>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(QuestDefinition definition, CancellationToken cancellationToken = default);
}
