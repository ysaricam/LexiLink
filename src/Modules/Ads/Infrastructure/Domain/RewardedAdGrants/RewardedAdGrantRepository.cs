using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Ads.Infrastructure.Domain.RewardedAdGrants;

internal class RewardedAdGrantRepository : IRewardedAdGrantRepository
{
    private readonly AdsContext _adsContext;

    internal RewardedAdGrantRepository(AdsContext adsContext)
    {
        _adsContext = adsContext;
    }

    public Task<RewardedAdGrant?> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return _adsContext.RewardedAdGrants
            .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);
    }

    public async Task<int> CountForPlayerSinceAsync(
        Guid playerId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
    {
        return await _adsContext.RewardedAdGrants
            .CountAsync(x => x.PlayerId == playerId && x.GrantedOn >= sinceUtc, cancellationToken);
    }

    public async Task AddAsync(RewardedAdGrant grant, CancellationToken cancellationToken = default)
    {
        await _adsContext.RewardedAdGrants.AddAsync(grant, cancellationToken);
    }
}
