using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;

internal class ClaimQuestCommandHandler : ICommandHandler<ClaimQuestCommand>
{
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IClock _clock;

    internal ClaimQuestCommandHandler(
        IPlayerQuestRepository playerQuestRepository,
        IClock clock)
    {
        _playerQuestRepository = playerQuestRepository;
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

        quest.Claim(_clock.UtcNow);
    }
}
