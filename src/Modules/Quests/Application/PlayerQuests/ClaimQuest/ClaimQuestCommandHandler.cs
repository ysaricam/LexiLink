using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;

internal class ClaimQuestCommandHandler : ICommandHandler<ClaimQuestCommand>
{
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IQuestCatalog _questCatalog;
    private readonly IQuestCounterReader _counterReader;
    private readonly IClock _clock;

    internal ClaimQuestCommandHandler(
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

    public async Task Handle(ClaimQuestCommand request, CancellationToken cancellationToken)
    {
        var quest = await _playerQuestRepository.GetByIdAsync(
            new PlayerQuestId(request.PlayerQuestId), cancellationToken);

        // Cross-player or missing id both surface as 404 to avoid leaking other players' quest ids.
        if (quest is null || quest.PlayerId != request.PlayerId)
        {
            throw new NotFoundException(nameof(PlayerQuest), request.PlayerQuestId);
        }

        // Even a deactivated definition allows claiming an already-issued
        // PlayerQuest — admin deactivation hides the row from /quests/me
        // listings but does not retroactively void earned rewards.
        var definition = await _questCatalog.ResolveAsync(quest.QuestDefinitionId, cancellationToken);
        if (definition is null)
        {
            throw new NotFoundException(nameof(QuestDefinition), quest.QuestDefinitionId.Value);
        }

        var now = _clock.UtcNow;
        var counters = await _counterReader.ReadAsync(request.PlayerId, now, cancellationToken);
        var isReadyToClaim = ComputeIsReadyToClaim(quest, definition, counters, now);

        quest.Claim(now, isReadyToClaim, definition.Reward);
    }

    private static bool ComputeIsReadyToClaim(
        PlayerQuest quest,
        QuestDefinition definition,
        QuestCounters counters,
        DateTime now)
    {
        if (quest.ExpiresAt is { } expiresAt && now >= expiresAt)
        {
            return false;
        }

        var currentCounter = definition.Trigger switch
        {
            QuestTrigger.GameCompletedDaily   => counters.GamesCompletedToday,
            QuestTrigger.GameCompletedTotal   => counters.GamesCompletedTotal,
            QuestTrigger.AuthProviderLinked   => counters.AuthProviderLinked ? 1 : 0,
            _                                 => 0,
        };

        return currentCounter - quest.ProgressBaselineSnapshot >= definition.Threshold;
    }
}
