namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GrantRewardedAdReward;

public enum RewardedAdGrantOutcome
{
    /// <summary>Verified, under cap, Diamond granted, ledger row recorded.</summary>
    Granted,

    /// <summary>The transaction id was already granted; no second grant.</summary>
    AlreadyGranted,

    /// <summary>The player has hit the daily cap; benign "no reward".</summary>
    DailyLimitReached,

    /// <summary>Signature invalid or <c>user_id</c> unusable; nothing granted.</summary>
    VerificationFailed
}

public sealed record GrantRewardedAdRewardResultDto(
    string Outcome,
    int DiamondAmount,
    int GrantsToday,
    int DailyLimit,
    int RemainingToday);
