using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal class QuestCatalog : IQuestCatalog
{
    private readonly IReadOnlyDictionary<QuestType, QuestDefinition> _definitions;

    public QuestCatalog()
    {
        var definitions = new[]
        {
            new QuestDefinition(
                Type: QuestType.FirstGameCompleted,
                Cadence: QuestCadence.OneTime,
                Goal: 1,
                RewardAmount: 3,
                PrerequisiteQuestType: null),
            new QuestDefinition(
                Type: QuestType.ThreeGamesCompleted,
                Cadence: QuestCadence.OneTime,
                Goal: 3,
                RewardAmount: 5,
                PrerequisiteQuestType: null),
            new QuestDefinition(
                Type: QuestType.AccountLinked,
                Cadence: QuestCadence.OneTime,
                Goal: 1,
                RewardAmount: 5,
                PrerequisiteQuestType: QuestType.ThreeGamesCompleted),
            new QuestDefinition(
                Type: QuestType.DailyThreeGames,
                Cadence: QuestCadence.Daily,
                Goal: 3,
                RewardAmount: 5,
                PrerequisiteQuestType: null),
        };

        _definitions = definitions.ToDictionary(d => d.Type);
    }

    public QuestDefinition Resolve(QuestType type)
    {
        if (!_definitions.TryGetValue(type, out var definition))
        {
            throw new InvalidOperationException(
                $"Quest type '{type}' is not registered in the catalog.");
        }

        return definition;
    }

    public IReadOnlyCollection<QuestDefinition> All => _definitions.Values.ToList();
}
