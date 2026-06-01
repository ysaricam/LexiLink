using LexiLink.Common.Domain;

namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants.Rules;

/// <summary>
/// Caps how many rewarded-ad Diamond grants a player can earn per day.
/// Checked by the grant handler against a count of the player's grants in
/// the current UTC day. Hitting the cap is a benign "no reward" outcome,
/// not a failure.
/// </summary>
public class RewardedAdDailyLimitRule : IBusinessRule
{
    private readonly int _grantsToday;
    private readonly int _dailyLimit;

    public RewardedAdDailyLimitRule(int grantsToday, int dailyLimit)
    {
        _grantsToday = grantsToday;
        _dailyLimit = dailyLimit;
    }

    public bool IsBroken() => _grantsToday >= _dailyLimit;

    public string Message => "Daily rewarded-ad limit reached.";
}
