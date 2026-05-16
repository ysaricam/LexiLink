using LexiLink.Modules.Energy.Domain.PlayerEnergies;
using LexiLink.Modules.Energy.Tests.SeedWork;

namespace LexiLink.Modules.Energy.Tests.PlayerEnergies;

public abstract class PlayerEnergyTestsBase : TestBase
{
    protected const int DefaultMaximumAmount = 5;
    protected const int DefaultRechargeIntervalSeconds = 900;

    protected static readonly DateTime FixedInitializedAt =
        new(2026, 5, 14, 9, 0, 0, DateTimeKind.Utc);

    protected static PlayerEnergy CreateAtMaximum(
        int maximumAmount = DefaultMaximumAmount,
        int rechargeIntervalSeconds = DefaultRechargeIntervalSeconds,
        DateTime? initializedAt = null)
    {
        return PlayerEnergy.InitializeFor(
            Guid.NewGuid(),
            maximumAmount,
            rechargeIntervalSeconds,
            initializedAt ?? FixedInitializedAt);
    }
}
