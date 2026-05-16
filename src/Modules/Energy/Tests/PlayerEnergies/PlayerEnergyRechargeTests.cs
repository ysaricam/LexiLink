using LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;

namespace LexiLink.Modules.Energy.Tests.PlayerEnergies;

[TestFixture]
public class PlayerEnergyRechargeTests : PlayerEnergyTestsBase
{
    [Test]
    public void Recharge_AtMaximum_DoesNothing()
    {
        var energy = CreateAtMaximum();
        var farFuture = FixedInitializedAt.AddDays(1);

        energy.RechargeBasedOnElapsedTime(farFuture);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount);
        energy.LastRefilledOn.Should().Be(FixedInitializedAt);
        AssertDomainEventNotPublished<PlayerEnergyRefilledDomainEvent>(energy);
    }

    [Test]
    public void Recharge_WithLessThanOneIntervalElapsed_DoesNothing()
    {
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(1);
        energy.Consume(2, consumedAt);

        var partialElapsed = consumedAt.AddSeconds(DefaultRechargeIntervalSeconds - 1);
        energy.RechargeBasedOnElapsedTime(partialElapsed);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount - 2);
        energy.LastRefilledOn.Should().Be(consumedAt);
    }

    [Test]
    public void Recharge_AfterOneInterval_GainsOneAndAdvancesTimerByOneInterval()
    {
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(1);
        energy.Consume(2, consumedAt);

        var oneIntervalLater = consumedAt.AddSeconds(DefaultRechargeIntervalSeconds);
        energy.RechargeBasedOnElapsedTime(oneIntervalLater);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount - 1);
        energy.LastRefilledOn.Should().Be(oneIntervalLater);
    }

    [Test]
    public void Recharge_AfterMultipleIntervals_PreservesPartialInterval()
    {
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(1);
        energy.Consume(3, consumedAt);

        var partialMultipleLater = consumedAt.AddSeconds(2 * DefaultRechargeIntervalSeconds + 100);
        energy.RechargeBasedOnElapsedTime(partialMultipleLater);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount - 1);
        energy.LastRefilledOn.Should().Be(consumedAt.AddSeconds(2 * DefaultRechargeIntervalSeconds));
    }

    [Test]
    public void Recharge_CapsAtMaximum_WhenMoreThanEnoughTimeElapsed()
    {
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(1);
        energy.Consume(2, consumedAt);

        var farFuture = consumedAt.AddDays(1);
        energy.RechargeBasedOnElapsedTime(farFuture);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount);
        var expectedLastRefilledOn = consumedAt.AddSeconds(2 * DefaultRechargeIntervalSeconds);
        energy.LastRefilledOn.Should().Be(expectedLastRefilledOn);
    }

    [Test]
    public void Recharge_RaisesPlayerEnergyRefilledDomainEvent_WithGainedAmount()
    {
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(1);
        energy.Consume(2, consumedAt);

        var twoIntervalsLater = consumedAt.AddSeconds(2 * DefaultRechargeIntervalSeconds);
        energy.RechargeBasedOnElapsedTime(twoIntervalsLater);

        var evt = AssertPublishedDomainEvent<PlayerEnergyRefilledDomainEvent>(energy);
        evt.PlayerId.Should().Be(energy.Id.Value);
        evt.GainedAmount.Should().Be(2);
        evt.CurrentAmount.Should().Be(DefaultMaximumAmount);
    }
}
