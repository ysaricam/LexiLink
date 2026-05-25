using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;

internal sealed class UpdateQuestDefinitionCommandHandler : ICommandHandler<UpdateQuestDefinitionCommand>
{
    private readonly IQuestDefinitionRepository _repository;

    internal UpdateQuestDefinitionCommandHandler(IQuestDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateQuestDefinitionCommand request, CancellationToken cancellationToken)
    {
        var selfId = new QuestDefinitionId(request.QuestDefinitionId);
        var definition = await _repository.GetByIdAsync(selfId, cancellationToken)
            ?? throw new NotFoundException(nameof(QuestDefinition), request.QuestDefinitionId);

        QuestDefinitionId? prereqId = null;
        var wouldCycle = false;
        if (request.PrerequisiteQuestDefinitionId is { } prereqGuid)
        {
            if (prereqGuid == request.QuestDefinitionId)
            {
                // Direct self-reference is the trivial cycle case. Handle
                // it explicitly so the chain walk below doesn't have to.
                wouldCycle = true;
            }
            else
            {
                prereqId = new QuestDefinitionId(prereqGuid);
                var prereq = await _repository.GetByIdAsync(prereqId, cancellationToken)
                    ?? throw new InvalidCommandException(new Dictionary<string, string[]>
                    {
                        [nameof(UpdateQuestDefinitionCommand.PrerequisiteQuestDefinitionId)] =
                            [$"Prerequisite QuestDefinition '{prereqGuid}' was not found."]
                    });

                wouldCycle = await WouldChainCycleAsync(prereq, selfId, cancellationToken);
            }
        }

        definition.Update(
            request.Description,
            request.Threshold,
            request.EnergyReward,
            request.HintReward,
            prereqId,
            request.ProgressBaseline,
            prerequisiteWouldCreateCycle: wouldCycle);
    }

    private async Task<bool> WouldChainCycleAsync(
        QuestDefinition prereq,
        QuestDefinitionId selfId,
        CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid> { prereq.Id.Value };
        var cursor = prereq.PrerequisiteQuestDefinitionId;

        while (cursor is not null)
        {
            if (cursor == selfId)
            {
                return true;
            }

            if (!visited.Add(cursor.Value))
            {
                // Pre-existing cycle in the catalog that does not pass
                // through self — leave that for the admin to clean up
                // separately; do not block this Update.
                return false;
            }

            var next = await _repository.GetByIdAsync(cursor, cancellationToken);
            if (next is null)
            {
                return false;
            }

            cursor = next.PrerequisiteQuestDefinitionId;
        }

        return false;
    }
}
