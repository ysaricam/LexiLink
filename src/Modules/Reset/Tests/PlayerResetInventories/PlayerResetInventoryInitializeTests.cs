using LexiLink.Modules.Reset.Domain.PlayerResetInventories;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;
using LexiLink.Modules.Reset.Tests.SeedWork;

namespace LexiLink.Modules.Reset.Tests.PlayerResetInventories;

[TestFixture]
public class PlayerResetInventoryInitializeTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");

    [Test]
    public void InitializeFor_WithZeroBalance_CreatesInventoryAtZero()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.Balance.Should().Be(0);
        inventory.Id.Value.Should().Be(SamplePlayerId);
    }

    [Test]
    public void InitializeFor_WithPositiveBalance_CarriesSeed()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        inventory.Balance.Should().Be(3);
    }

    [Test]
    public void InitializeFor_RaisesInitializedDomainEvent_WithInitialBalance()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 2);

        var evt = AssertPublishedDomainEvent<PlayerResetInventoryInitializedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.InitialBalance.Should().Be(2);
    }

    [Test]
    public void InitializeFor_NegativeBalance_BreaksRule()
    {
        AssertBrokenRule<ResetAmountMustBeNonNegativeRule>(() =>
            PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: -1));
    }
}
