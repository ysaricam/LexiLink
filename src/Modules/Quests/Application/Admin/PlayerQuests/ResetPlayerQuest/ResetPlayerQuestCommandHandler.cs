using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.Admin.PlayerQuests.ResetPlayerQuest;

internal sealed class ResetPlayerQuestCommandHandler : ICommandHandler<ResetPlayerQuestCommand>
{
    private readonly IPlayerQuestRepository _repository;
    private readonly IQuestCatalog _catalog;
    private readonly IClock _clock;

    internal ResetPlayerQuestCommandHandler(
        IPlayerQuestRepository repository,
        IQuestCatalog catalog,
        IClock clock)
    {
        _repository = repository;
        _catalog = catalog;
        _clock = clock;
    }

    public async Task Handle(ResetPlayerQuestCommand request, CancellationToken cancellationToken)
    {
        var quest = await _repository.GetByIdAsync(
            new PlayerQuestId(request.PlayerQuestId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerQuest), request.PlayerQuestId);

        // Refresh expiry from the current definition's cadence — Daily
        // quests get a fresh "next UTC midnight" window, OneTime stays
        // open-ended. Definition may be deactivated (null) → reset still
        // proceeds with no expiry.
        var now = _clock.UtcNow;
        var definition = await _catalog.ResolveAsync(quest.QuestType, cancellationToken);
        var newExpiresAt = definition?.Cadence == QuestCadence.Daily
            ? (DateTime?)NextUtcMidnight(now)
            : null;

        quest.AdminReset(now, newExpiresAt);
    }

    private static DateTime NextUtcMidnight(DateTime now)
    {
        var todayUtcMidnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        return todayUtcMidnight.AddDays(1);
    }
}
