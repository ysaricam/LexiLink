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
        var definition = await _repository.GetByIdAsync(
            new QuestDefinitionId(request.QuestDefinitionId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(QuestDefinition), request.QuestDefinitionId);

        definition.Update(request.Goal, request.RewardAmount, request.PrerequisiteQuestType);
    }
}
