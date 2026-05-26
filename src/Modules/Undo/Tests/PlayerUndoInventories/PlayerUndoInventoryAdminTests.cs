using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;
using LexiLink.Modules.Undo.Tests.SeedWork;

namespace LexiLink.Modules.Undo.Tests.PlayerUndoInventories;

[TestFixture]
public class PlayerUndoInventoryAdminTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SampleNow = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void AdminSet_OverridesBalanceToExactValue()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 5);
        inventory.ClearDomainEvents();

        inventory.AdminSet(newBalance: 12, now: SampleNow);

        inventory.Balance.Should().Be(12);
        var evt = AssertPublishedDomainEvent<PlayerUndoAdminSetDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.NewBalance.Should().Be(12);
        evt.SetOn.Should().Be(SampleNow);
    }

    [Test]
    public void AdminSet_ToZero_IsAllowed()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 5);

        inventory.AdminSet(newBalance: 0, now: SampleNow);

        inventory.Balance.Should().Be(0);
    }

    [Test]
    public void AdminSet_Negative_BreaksRule()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 5);

        AssertBrokenRule<UndoAmountMustBeNonNegativeRule>(() =>
            inventory.AdminSet(newBalance: -1, now: SampleNow));
    }

    [Test]
    public void AdminReset_SnapsBalanceToZeroAndRaisesEvent()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 7);
        inventory.ClearDomainEvents();

        inventory.AdminReset(now: SampleNow);

        inventory.Balance.Should().Be(0);
        var evt = AssertPublishedDomainEvent<PlayerUndoAdminResetDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.ResetOn.Should().Be(SampleNow);
    }

    [Test]
    public void AdminReset_FromZero_StaysAtZero()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.AdminReset(now: SampleNow);

        inventory.Balance.Should().Be(0);
    }
}
