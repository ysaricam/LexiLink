using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

public interface IPlayerQuestRepository : IRepository<PlayerQuest>
{
    Task<PlayerQuest?> GetByIdAsync(PlayerQuestId id, CancellationToken cancellationToken = default);

    Task<PlayerQuest?> GetActiveOrClaimedByPlayerAndDefinitionAsync(
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PlayerQuest>> GetByPlayerAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);

    Task<bool> HasClaimedAsync(
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(PlayerQuest playerQuest, CancellationToken cancellationToken = default);
}
