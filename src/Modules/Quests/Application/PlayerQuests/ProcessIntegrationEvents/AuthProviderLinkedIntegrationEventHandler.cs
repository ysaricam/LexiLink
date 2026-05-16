using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;
using LexiLink.Modules.Quests.Application.PlayerQuests.RecordQuestProgress;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ProcessIntegrationEvents;

internal class AuthProviderLinkedIntegrationEventHandler :
    IIntegrationEventHandler<AuthProviderLinkedIntegrationEvent>
{
    private readonly IQuestsModule _questsModule;

    internal AuthProviderLinkedIntegrationEventHandler(IQuestsModule questsModule)
    {
        _questsModule = questsModule;
    }

    public async Task Handle(AuthProviderLinkedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Issue is idempotent and silently no-ops when the AccountLinked prerequisite
        // (ThreeGamesCompleted claimed) is not yet satisfied.
        await _questsModule.ExecuteCommandAsync(
            new IssueQuestCommand(integrationEvent.PlayerId, QuestType.AccountLinked),
            cancellationToken);

        await _questsModule.ExecuteCommandAsync(
            new RecordQuestProgressCommand(integrationEvent.PlayerId, QuestType.AccountLinked, delta: 1),
            cancellationToken);
    }
}
