using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ProcessIntegrationEvents;

/// On player registration, issue every active quest definition. The
/// per-definition <see cref="IssueQuestCommand"/> handler is
/// idempotent: it no-ops when the prerequisite quest is not yet
/// claimed (so <c>AccountLinked</c> stays dormant until
/// <c>ThreeGamesCompleted</c> is claimed), when an active row already
/// exists, and when a one-time quest has already been claimed.
internal class PlayerRegisteredIntegrationEventHandler :
    IIntegrationEventHandler<PlayerRegisteredIntegrationEvent>
{
    private readonly IQuestsModule _questsModule;
    private readonly IQuestDefinitionRepository _questDefinitionRepository;

    internal PlayerRegisteredIntegrationEventHandler(
        IQuestsModule questsModule,
        IQuestDefinitionRepository questDefinitionRepository)
    {
        _questsModule = questsModule;
        _questDefinitionRepository = questDefinitionRepository;
    }

    public async Task Handle(
        PlayerRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var definitions = await _questDefinitionRepository.GetAllAsync(cancellationToken);

        foreach (var definition in definitions)
        {
            if (!definition.IsActive)
            {
                continue;
            }

            await _questsModule.ExecuteCommandAsync(
                new IssueQuestCommand(integrationEvent.PlayerId, definition.QuestType),
                cancellationToken);
        }
    }
}
