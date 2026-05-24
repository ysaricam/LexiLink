using LexiLink.Modules.Quests.Domain.PlayerQuests;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Quests.Infrastructure.Domain.PlayerQuests;

internal class PlayerQuestRepository : IPlayerQuestRepository
{
    private readonly QuestsContext _questsContext;

    internal PlayerQuestRepository(QuestsContext questsContext)
    {
        _questsContext = questsContext;
    }

    public async Task<PlayerQuest?> GetByIdAsync(PlayerQuestId id, CancellationToken cancellationToken = default)
    {
        return await _questsContext.PlayerQuests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PlayerQuest?> GetActiveOrClaimedByPlayerAndDefinitionAsync(
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        CancellationToken cancellationToken = default)
    {
        return await _questsContext.PlayerQuests
            .Where(x => EF.Property<Guid>(x, "_playerId") == playerId
                        && EF.Property<QuestDefinitionId>(x, "_questDefinitionId") == questDefinitionId)
            .OrderByDescending(x => EF.Property<DateTime>(x, "_issuedAt"))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PlayerQuest>> GetByPlayerAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        return await _questsContext.PlayerQuests
            .Where(x => EF.Property<Guid>(x, "_playerId") == playerId)
            .OrderByDescending(x => EF.Property<DateTime>(x, "_issuedAt"))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasClaimedAsync(
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        CancellationToken cancellationToken = default)
    {
        return await _questsContext.PlayerQuests
            .AnyAsync(x => EF.Property<Guid>(x, "_playerId") == playerId
                            && EF.Property<QuestDefinitionId>(x, "_questDefinitionId") == questDefinitionId
                            && EF.Property<QuestState>(x, "_state") == QuestState.Claimed,
                cancellationToken);
    }

    public async Task AddAsync(PlayerQuest playerQuest, CancellationToken cancellationToken = default)
    {
        await _questsContext.PlayerQuests.AddAsync(playerQuest, cancellationToken);
    }
}
