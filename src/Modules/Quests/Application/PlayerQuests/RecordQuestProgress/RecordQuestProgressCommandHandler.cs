using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Commands;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.RecordQuestProgress;

internal class RecordQuestProgressCommandHandler : ICommandHandler<RecordQuestProgressCommand>
{
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IClock _clock;

    internal RecordQuestProgressCommandHandler(
        IPlayerQuestRepository playerQuestRepository,
        IClock clock)
    {
        _playerQuestRepository = playerQuestRepository;
        _clock = clock;
    }

    public async Task Handle(RecordQuestProgressCommand request, CancellationToken cancellationToken)
    {
        var quest = await _playerQuestRepository.GetActiveOrReadyByPlayerAndTypeAsync(
            request.PlayerId, request.QuestType, cancellationToken);

        if (quest is null || quest.State != QuestState.Active)
        {
            return;
        }

        quest.RecordProgress(request.Delta, _clock.UtcNow);
    }
}
