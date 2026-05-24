using LexiLink.Modules.Hint.Domain.PlayerHintInventories;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;
using LexiLink.Modules.Hint.Tests.SeedWork;

namespace LexiLink.Modules.Hint.Tests.PlayerHintInventories;

[TestFixture]
public class PlayerHintInventoryInitializeTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");

    [Test]
    public void InitializeFor_WithZeroBalance_CreatesInventoryAtZero()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.Balance.Should().Be(0);
        inventory.Id.Value.Should().Be(SamplePlayerId);
    }

    [Test]
    public void InitializeFor_WithPositiveBalance_CarriesSeed()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        inventory.Balance.Should().Be(3);
    }

    [Test]
    public void InitializeFor_RaisesInitializedDomainEvent_WithInitialBalance()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 2);

        var evt = AssertPublishedDomainEvent<PlayerHintInventoryInitializedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.InitialBalance.Should().Be(2);
    }

    [Test]
    public void InitializeFor_NegativeBalance_BreaksRule()
    {
        AssertBrokenRule<HintAmountMustBeNonNegativeRule>(() =>
            PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: -1));
    }
}
