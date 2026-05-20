using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.IssueQuest;

internal class IssueQuestCommandHandler : ICommandHandler<IssueQuestCommand>
{
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IQuestCatalog _questCatalog;
    private readonly IClock _clock;

    internal IssueQuestCommandHandler(
        IPlayerQuestRepository playerQuestRepository,
        IQuestCatalog questCatalog,
        IClock clock)
    {
        _playerQuestRepository = playerQuestRepository;
        _questCatalog = questCatalog;
        _clock = clock;
    }

    public async Task Handle(IssueQuestCommand request, CancellationToken cancellationToken)
    {
        var definition = await _questCatalog.ResolveAsync(request.QuestType, cancellationToken);

        // No active definition for this type — quest was deactivated or
        // never seeded. Issuance is a no-op (existing PlayerQuest rows
        // are untouched).
        if (definition is null)
        {
            return;
        }

        if (definition.PrerequisiteQuestType is { } prereq)
        {
            var prereqClaimed = await _playerQuestRepository.HasClaimedAsync(
                request.PlayerId, prereq, cancellationToken);
            if (!prereqClaimed)
            {
                return;
            }
        }

        var existing = await _playerQuestRepository.GetActiveOrReadyByPlayerAndTypeAsync(
            request.PlayerId, request.QuestType, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = _clock.UtcNow;

        if (definition.Cadence == QuestCadence.OneTime)
        {
            var alreadyClaimed = await _playerQuestRepository.HasClaimedAsync(
                request.PlayerId, request.QuestType, cancellationToken);
            if (alreadyClaimed)
            {
                return;
            }
        }

        var expiresAt = definition.Cadence == QuestCadence.Daily
            ? (DateTime?)NextUtcMidnight(now)
            : null;

        var quest = PlayerQuest.IssueFor(
            request.PlayerId,
            request.QuestType,
            definition.Goal,
            definition.RewardAmount,
            now,
            expiresAt);

        await _playerQuestRepository.AddAsync(quest, cancellationToken);
    }

    private static DateTime NextUtcMidnight(DateTime now)
    {
        var todayUtcMidnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        return todayUtcMidnight.AddDays(1);
    }
}
