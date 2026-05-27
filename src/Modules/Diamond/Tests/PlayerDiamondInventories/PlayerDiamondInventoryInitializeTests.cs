using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Rules;
using LexiLink.Modules.Diamond.Tests.SeedWork;

namespace LexiLink.Modules.Diamond.Tests.PlayerDiamondInventories;

[TestFixture]
public class PlayerDiamondInventoryInitializeTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");

    [Test]
    public void InitializeFor_WithZeroBalance_CreatesInventoryAtZero()
    {
        var inventory = PlayerDiamondInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.Balance.Should().Be(0);
        inventory.Id.Value.Should().Be(SamplePlayerId);
    }

    [Test]
    public void InitializeFor_WithPositiveBalance_CarriesSeed()
    {
        var inventory = PlayerDiamondInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        inventory.Balance.Should().Be(3);
    }

    [Test]
    public void InitializeFor_RaisesInitializedDomainEvent_WithInitialBalance()
    {
        var inventory = PlayerDiamondInventory.InitializeFor(SamplePlayerId, initialBalance: 2);

        var evt = AssertPublishedDomainEvent<PlayerDiamondInventoryInitializedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.InitialBalance.Should().Be(2);
    }

    [Test]
    public void InitializeFor_NegativeBalance_BreaksRule()
    {
        AssertBrokenRule<DiamondAmountMustBeNonNegativeRule>(() =>
            PlayerDiamondInventory.InitializeFor(SamplePlayerId, initialBalance: -1));
    }
}
