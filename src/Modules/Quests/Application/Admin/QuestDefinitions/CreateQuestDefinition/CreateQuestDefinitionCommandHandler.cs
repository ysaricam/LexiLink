using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;

internal sealed class CreateQuestDefinitionCommandHandler
    : ICommandHandler<CreateQuestDefinitionCommand, Guid>
{
    private readonly IQuestDefinitionRepository _repository;

    internal CreateQuestDefinitionCommandHandler(IQuestDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateQuestDefinitionCommand request, CancellationToken cancellationToken)
    {
        // QuestType is unique in the catalog — one active definition per
        // type. Recreating an existing type returns a 400 ValidationProblem
        // (InvalidCommandException). To re-tune, use UpdateQuestDefinition.
        var existing = await _repository.GetByQuestTypeAsync(request.QuestType, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidCommandException(new Dictionary<string, string[]>
            {
                [nameof(CreateQuestDefinitionCommand.QuestType)] =
                    [$"A QuestDefinition for type '{request.QuestType}' already exists."]
            });
        }

        var definition = QuestDefinition.Create(
            request.QuestType,
            request.Cadence,
            request.Goal,
            request.RewardAmount,
            request.PrerequisiteQuestType);

        await _repository.AddAsync(definition, cancellationToken);
        return definition.Id.Value;
    }
}
