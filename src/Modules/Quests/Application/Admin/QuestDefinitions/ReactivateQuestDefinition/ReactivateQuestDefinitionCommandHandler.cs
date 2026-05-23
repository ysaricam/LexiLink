using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.ReactivateQuestDefinition;

internal sealed class ReactivateQuestDefinitionCommandHandler : ICommandHandler<ReactivateQuestDefinitionCommand>
{
    private readonly IQuestDefinitionRepository _repository;

    internal ReactivateQuestDefinitionCommandHandler(IQuestDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ReactivateQuestDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetByIdAsync(
            new QuestDefinitionId(request.QuestDefinitionId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(QuestDefinition), request.QuestDefinitionId);

        definition.Reactivate();
    }
}
