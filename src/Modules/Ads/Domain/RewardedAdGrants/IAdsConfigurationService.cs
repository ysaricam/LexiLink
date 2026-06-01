namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants;

public interface IAdsConfigurationService
{
    /// <summary>
    /// Backend-owned Diamond amount granted per verified rewarded ad.
    /// Operator-tunable via <c>Ads:RewardedDiamondAmount</c>; defaults to 5.
    /// The ad-network/client reward value is never trusted.
    /// </summary>
    int RewardedDiamondAmount { get; }

    /// <summary>
    /// Maximum number of rewarded-ad Diamond grants a player can earn per
    /// UTC day. Operator-tunable via <c>Ads:RewardedDailyLimit</c>;
    /// defaults to 10.
    /// </summary>
    int RewardedDailyLimit { get; }
}
