using LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

namespace LexiLink.Modules.Energy.Tests.PlayerEnergies;

[TestFixture]
public class PlayerEnergyGrantBonusTests : PlayerEnergyTestsBase
{
    [Test]
    public void GrantBonus_FromMaximum_PushesCurrentAboveMaximum()
    {
        var energy = CreateAtMaximum();

        energy.GrantBonus(3, FixedInitializedAt.AddSeconds(60));

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount + 3);
    }

    [Test]
    public void GrantBonus_DoesNotResetRefillTimer()
    {
        var energy = CreateAtMaximum();
        var bonusAt = FixedInitializedAt.AddSeconds(60);

        energy.GrantBonus(2, bonusAt);

        energy.LastRefilledOn.Should().Be(FixedInitializedAt,
            "bonus must not affect the recharge cycle");
    }

    [Test]
    public void GrantBonus_WhenAmountIsZero_BreaksBonusAmountMustBePositiveRule()
    {
        var energy = CreateAtMaximum();

        AssertBrokenRule<BonusAmountMustBePositiveRule>(() =>
            energy.GrantBonus(0, FixedInitializedAt.AddSeconds(1)));
    }

    [Test]
    public void GrantBonus_WhenAmountIsNegative_BreaksBonusAmountMustBePositiveRule()
    {
        var energy = CreateAtMaximum();

        AssertBrokenRule<BonusAmountMustBePositiveRule>(() =>
            energy.GrantBonus(-1, FixedInitializedAt.AddSeconds(1)));
    }

    [Test]
    public void ConsumeFromOverMax_StayingOverMax_DoesNotSetRefillTimer()
    {
        // Bring energy to 10/5 via bonus, then consume 1 → 9/5.
        var energy = CreateAtMaximum();
        energy.GrantBonus(5, FixedInitializedAt.AddSeconds(60));

        var consumedAt = FixedInitializedAt.AddSeconds(120);
        energy.Consume(1, consumedAt);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount + 4);
        energy.LastRefilledOn.Should().Be(FixedInitializedAt,
            "consume that leaves balance above max must not arm the recharge timer");
    }

    [Test]
    public void ConsumeFromOverMax_LandingExactlyAtMax_DoesNotSetRefillTimer()
    {
        // 6/5 → consume 1 → 5/5.
        var energy = CreateAtMaximum();
        energy.GrantBonus(1, FixedInitializedAt.AddSeconds(60));

        var consumedAt = FixedInitializedAt.AddSeconds(120);
        energy.Consume(1, consumedAt);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount);
        energy.LastRefilledOn.Should().Be(FixedInitializedAt,
            "consume that lands at exactly max must keep the timer idle");
    }

    [Test]
    public void ConsumeFromMax_LandingBelowMax_SetsRefillTimer()
    {
        // 5/5 → consume 1 → 4/5.
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(60);

        energy.Consume(1, consumedAt);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount - 1);
        energy.LastRefilledOn.Should().Be(consumedAt,
            "consume that drops below max arms the recharge timer");
    }
}
