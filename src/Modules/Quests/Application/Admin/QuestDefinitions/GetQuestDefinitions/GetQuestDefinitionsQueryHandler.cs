using LexiLink.Modules.Quests.Application.Configuration.Queries;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;

internal sealed class GetQuestDefinitionsQueryHandler
    : IQueryHandler<GetQuestDefinitionsQuery, IReadOnlyList<QuestDefinitionDto>>
{
    private readonly IQuestDefinitionRepository _repository;

    internal GetQuestDefinitionsQueryHandler(IQuestDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<QuestDefinitionDto>> Handle(
        GetQuestDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var all = await _repository.GetAllAsync(cancellationToken);
        return all
            .Select(d => new QuestDefinitionDto(
                d.Id.Value,
                d.QuestType,
                d.Cadence,
                d.Goal,
                d.RewardAmount,
                d.PrerequisiteQuestType,
                d.IsActive))
            .ToList();
    }
}
