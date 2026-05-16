using LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;
using LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

namespace LexiLink.Modules.Energy.Tests.PlayerEnergies;

[TestFixture]
public class PlayerEnergyConsumeTests : PlayerEnergyTestsBase
{
    [Test]
    public void Consume_FromMaximum_DecrementsAndResetsRefillTimer()
    {
        var energy = CreateAtMaximum();
        var consumedAt = FixedInitializedAt.AddSeconds(60);

        energy.Consume(1, consumedAt);

        energy.CurrentAmount.Should().Be(DefaultMaximumAmount - 1);
        energy.LastRefilledOn.Should().Be(consumedAt);
    }

    [Test]
    public void Consume_FromMaximum_RaisesPlayerEnergyConsumedDomainEvent()
    {
        var energy = CreateAtMaximum();

        energy.Consume(1, FixedInitializedAt.AddSeconds(60));

        var evt = AssertPublishedDomainEvent<PlayerEnergyConsumedDomainEvent>(energy);
        evt.PlayerId.Should().Be(energy.Id.Value);
        evt.Amount.Should().Be(1);
        evt.RemainingAmount.Should().Be(DefaultMaximumAmount - 1);
    }

    [Test]
    public void Consume_FromBelowMaximum_DoesNotResetRefillTimer()
    {
        var energy = CreateAtMaximum();
        var firstConsumeAt = FixedInitializedAt.AddSeconds(60);
        energy.Consume(1, firstConsumeAt);

        var secondConsumeAt = firstConsumeAt.AddSeconds(120);
        energy.Consume(1, secondConsumeAt);

        energy.LastRefilledOn.Should().Be(firstConsumeAt);
        energy.CurrentAmount.Should().Be(DefaultMaximumAmount - 2);
    }

    [Test]
    public void Consume_WhenInsufficientEnergy_BreaksEnergyMustBeSufficientToConsumeRule()
    {
        var energy = CreateAtMaximum(maximumAmount: 1);
        energy.Consume(1, FixedInitializedAt.AddSeconds(1));

        AssertBrokenRule<EnergyMustBeSufficientToConsumeRule>(() =>
            energy.Consume(1, FixedInitializedAt.AddSeconds(2)));
    }

    [Test]
    public void Consume_WhenAmountIsNegative_BreaksEnergyAmountCannotBeNegativeRule()
    {
        var energy = CreateAtMaximum();

        AssertBrokenRule<EnergyAmountCannotBeNegativeRule>(() =>
            energy.Consume(-1, FixedInitializedAt.AddSeconds(1)));
    }
}
