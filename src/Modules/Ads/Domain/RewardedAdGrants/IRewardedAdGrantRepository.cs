using LexiLink.Common.Domain;

namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants;

public interface IRewardedAdGrantRepository : IRepository<RewardedAdGrant>
{
    /// <summary>Idempotency lookup by the AdMob SSV transaction id.</summary>
    Task<RewardedAdGrant?> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>Counts a player's grants on/after <paramref name="sinceUtc"/> — used for the daily cap.</summary>
    Task<int> CountForPlayerSinceAsync(
        Guid playerId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default);

    Task AddAsync(RewardedAdGrant grant, CancellationToken cancellationToken = default);
}
