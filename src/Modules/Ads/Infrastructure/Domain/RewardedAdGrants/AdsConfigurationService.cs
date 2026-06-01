using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using Microsoft.Extensions.Configuration;

namespace LexiLink.Modules.Ads.Infrastructure.Domain.RewardedAdGrants;

internal sealed class AdsConfigurationService : IAdsConfigurationService
{
    private const int DefaultRewardedDiamondAmount = 5;
    private const int DefaultRewardedDailyLimit = 10;

    public AdsConfigurationService(IConfiguration configuration)
    {
        RewardedDiamondAmount = ReadInt(
            configuration, "Ads:RewardedDiamondAmount", DefaultRewardedDiamondAmount);
        RewardedDailyLimit = ReadInt(
            configuration, "Ads:RewardedDailyLimit", DefaultRewardedDailyLimit);
    }

    public int RewardedDiamondAmount { get; }

    public int RewardedDailyLimit { get; }

    private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var value) ? value : defaultValue;
    }
}
