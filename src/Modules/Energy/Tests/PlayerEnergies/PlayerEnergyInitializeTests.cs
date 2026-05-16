using LexiLink.Modules.Energy.Domain.PlayerEnergies;
using LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

namespace LexiLink.Modules.Energy.Tests.PlayerEnergies;

[TestFixture]
public class PlayerEnergyInitializeTests : PlayerEnergyTestsBase
{
    [Test]
    public void InitializeFor_WithValidConfiguration_StartsAtMaximum()
    {
        var playerId = Guid.NewGuid();

        var energy = PlayerEnergy.InitializeFor(
            playerId, DefaultMaximumAmount, DefaultRechargeIntervalSeconds, FixedInitializedAt);

        energy.Id.Value.Should().Be(playerId);
        energy.CurrentAmount.Should().Be(DefaultMaximumAmount);
        energy.MaximumAmount.Should().Be(DefaultMaximumAmount);
        energy.RechargeIntervalSeconds.Should().Be(DefaultRechargeIntervalSeconds);
        energy.LastRefilledOn.Should().Be(FixedInitializedAt);
    }

    [Test]
    public void InitializeFor_DoesNotRaiseDomainEventOnFirstInitialization()
    {
        var energy = CreateAtMaximum();

        AssertDomainEventNotPublished<Domain.PlayerEnergies.Events.PlayerEnergyRefilledDomainEvent>(energy);
        AssertDomainEventNotPublished<Domain.PlayerEnergies.Events.PlayerEnergyConsumedDomainEvent>(energy);
    }

    [Test]
    public void InitializeFor_WhenMaximumIsZero_BreaksEnergyConfigurationMustBeValidRule()
    {
        AssertBrokenRule<EnergyConfigurationMustBeValidRule>(() =>
            PlayerEnergy.InitializeFor(Guid.NewGuid(), 0, DefaultRechargeIntervalSeconds, FixedInitializedAt));
    }

    [Test]
    public void InitializeFor_WhenMaximumIsNegative_BreaksEnergyConfigurationMustBeValidRule()
    {
        AssertBrokenRule<EnergyConfigurationMustBeValidRule>(() =>
            PlayerEnergy.InitializeFor(Guid.NewGuid(), -1, DefaultRechargeIntervalSeconds, FixedInitializedAt));
    }

    [Test]
    public void InitializeFor_WhenRechargeIntervalIsZero_BreaksEnergyConfigurationMustBeValidRule()
    {
        AssertBrokenRule<EnergyConfigurationMustBeValidRule>(() =>
            PlayerEnergy.InitializeFor(Guid.NewGuid(), DefaultMaximumAmount, 0, FixedInitializedAt));
    }
}
