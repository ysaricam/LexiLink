namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GetRewardedAdStatus;

/// <summary>
/// The player's rewarded-ad standing for the current UTC day: how many
/// grants they have already earned, the daily cap, how many remain, and the
/// backend-owned Diamond payout per ad.
/// </summary>
public sealed record RewardedAdStatusDto(
    int GrantsToday,
    int DailyLimit,
    int RemainingToday,
    int DiamondPerAd);
