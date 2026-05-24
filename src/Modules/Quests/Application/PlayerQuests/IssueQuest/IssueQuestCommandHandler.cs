using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;

internal class IssueQuestCommandHandler : ICommandHandler<IssueQuestCommand>
{
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IQuestCatalog _questCatalog;
    private readonly IQuestCounterReader _counterReader;
    private readonly IClock _clock;

    internal IssueQuestCommandHandler(
        IPlayerQuestRepository playerQuestRepository,
        IQuestCatalog questCatalog,
        IQuestCounterReader counterReader,
        IClock clock)
    {
        _playerQuestRepository = playerQuestRepository;
        _questCatalog = questCatalog;
        _counterReader = counterReader;
        _clock = clock;
    }

    public async Task Handle(IssueQuestCommand request, CancellationToken cancellationToken)
    {
        var definitionId = new QuestDefinitionId(request.QuestDefinitionId);
        var definition = await _questCatalog.ResolveAsync(definitionId, cancellationToken);

        if (definition is null)
        {
            return;
        }

        if (definition.PrerequisiteQuestDefinitionId is { } prereqId)
        {
            var prereqClaimed = await _playerQuestRepository.HasClaimedAsync(
                request.PlayerId, prereqId, cancellationToken);
            if (!prereqClaimed)
            {
                return;
            }
        }

        var existing = await _playerQuestRepository.GetActiveOrClaimedByPlayerAndDefinitionAsync(
            request.PlayerId, definitionId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = _clock.UtcNow;
        var counters = await _counterReader.ReadAsync(request.PlayerId, now, cancellationToken);
        var baseline = ComputeBaseline(definition, counters);
        var expiresAt = definition.Trigger == QuestTrigger.GameCompletedDaily
            ? (DateTime?)NextUtcMidnight(now)
            : null;

        var quest = PlayerQuest.IssueFor(
            request.PlayerId,
            definitionId,
            baseline,
            now,
            expiresAt);

        await _playerQuestRepository.AddAsync(quest, cancellationToken);
    }

    private static int ComputeBaseline(QuestDefinition definition, QuestCounters counters) =>
        definition.Trigger switch
        {
            // Daily counter rolls over at UTC midnight, so the "snapshot"
            // is effectively today's count at issue time. For freshly
            // issued daily quests this is usually 0, but if the player
            // completed games earlier today before the daily quest was
            // (re-)issued, we still measure progress from now forward.
            QuestTrigger.GameCompletedDaily => counters.GamesCompletedToday,

            // Auth-linked is binary; no baseline math needed. Store 0.
            QuestTrigger.AuthProviderLinked => 0,

            // Total counter: per ProgressBaseline, either snapshot the
            // current value (so the threshold counts new games only) or
            // start from zero (so the threshold counts the player's
            // existing lifetime games — useful for retroactive
            // milestones).
            QuestTrigger.GameCompletedTotal =>
                definition.ProgressBaseline == ProgressBaseline.FromExistingTotal
                    ? 0
                    : counters.GamesCompletedTotal,

            _ => 0,
        };

    private static DateTime NextUtcMidnight(DateTime now)
    {
        var todayUtcMidnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        return todayUtcMidnight.AddDays(1);
    }
}
