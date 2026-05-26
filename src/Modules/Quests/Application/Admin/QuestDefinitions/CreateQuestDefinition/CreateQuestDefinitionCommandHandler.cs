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
        QuestDefinitionId? prereqId = null;
        if (request.PrerequisiteQuestDefinitionId is { } prereqGuid)
        {
            prereqId = new QuestDefinitionId(prereqGuid);
            var prereq = await _repository.GetByIdAsync(prereqId, cancellationToken)
                ?? throw new InvalidCommandException(new Dictionary<string, string[]>
                {
                    [nameof(CreateQuestDefinitionCommand.PrerequisiteQuestDefinitionId)] =
                        [$"Prerequisite QuestDefinition '{prereqGuid}' was not found."]
                });
            // Defensive: a freshly created definition cannot already be
            // referenced by anything, so a cycle through self is
            // impossible here. The prereq's own chain is the admin's
            // problem to keep clean.
            _ = prereq;
        }

        var definition = QuestDefinition.Create(
            request.Name,
            request.Description,
            request.Trigger,
            request.Threshold,
            request.EnergyReward,
            request.HintReward,
            request.UndoReward,
            request.ResetReward,
            prereqId,
            request.ProgressBaseline,
            prerequisiteWouldCreateCycle: false);

        await _repository.AddAsync(definition, cancellationToken);
        return definition.Id.Value;
    }
}
