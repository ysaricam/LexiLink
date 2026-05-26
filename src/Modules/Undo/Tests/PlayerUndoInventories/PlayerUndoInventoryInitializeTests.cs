using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;
using LexiLink.Modules.Undo.Tests.SeedWork;

namespace LexiLink.Modules.Undo.Tests.PlayerUndoInventories;

[TestFixture]
public class PlayerUndoInventoryInitializeTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");

    [Test]
    public void InitializeFor_WithZeroBalance_CreatesInventoryAtZero()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.Balance.Should().Be(0);
        inventory.Id.Value.Should().Be(SamplePlayerId);
    }

    [Test]
    public void InitializeFor_WithPositiveBalance_CarriesSeed()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        inventory.Balance.Should().Be(3);
    }

    [Test]
    public void InitializeFor_RaisesInitializedDomainEvent_WithInitialBalance()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 2);

        var evt = AssertPublishedDomainEvent<PlayerUndoInventoryInitializedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.InitialBalance.Should().Be(2);
    }

    [Test]
    public void InitializeFor_NegativeBalance_BreaksRule()
    {
        AssertBrokenRule<UndoAmountMustBeNonNegativeRule>(() =>
            PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: -1));
    }
}
