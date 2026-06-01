using LexiLink.Modules.Ads.Application.Contracts;

namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GetRewardedAdStatus;

public sealed class GetRewardedAdStatusQuery : QueryBase<RewardedAdStatusDto>
{
    public GetRewardedAdStatusQuery(Guid playerId)
    {
        PlayerId = playerId;
    }

    public Guid PlayerId { get; }
}
