using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;
using LexiLink.Modules.Quests.Application.PlayerQuests.RecordQuestProgress;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ProcessIntegrationEvents;

internal class GameCompletedIntegrationEventHandler :
    IIntegrationEventHandler<GameCompletedIntegrationEvent>
{
    private static readonly QuestType[] TriggeredQuestTypes =
    {
        QuestType.FirstGameCompleted,
        QuestType.ThreeGamesCompleted,
        QuestType.DailyThreeGames,
    };

    private readonly IQuestsModule _questsModule;

    internal GameCompletedIntegrationEventHandler(IQuestsModule questsModule)
    {
        _questsModule = questsModule;
    }

    public async Task Handle(GameCompletedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        foreach (var questType in TriggeredQuestTypes)
        {
            await _questsModule.ExecuteCommandAsync(
                new IssueQuestCommand(integrationEvent.PlayerId, questType),
                cancellationToken);

            await _questsModule.ExecuteCommandAsync(
                new RecordQuestProgressCommand(integrationEvent.PlayerId, questType, delta: 1),
                cancellationToken);
        }
    }
}
