using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal sealed class QuestCatalog : IQuestCatalog
{
    private readonly IQuestDefinitionRepository _repository;

    internal QuestCatalog(IQuestDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<QuestDefinition?> ResolveAsync(QuestDefinitionId questDefinitionId, CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetByIdAsync(questDefinitionId, cancellationToken);
        return definition is { IsActive: true } ? definition : null;
    }

    public async Task<IReadOnlyList<QuestDefinition>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(cancellationToken);
        return all.Where(d => d.IsActive).ToList();
    }
}
