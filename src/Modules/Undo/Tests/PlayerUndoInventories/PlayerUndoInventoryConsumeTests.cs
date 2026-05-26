using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;
using LexiLink.Modules.Undo.Tests.SeedWork;

namespace LexiLink.Modules.Undo.Tests.PlayerUndoInventories;

[TestFixture]
public class PlayerUndoInventoryConsumeTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SampleNow = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Consume_FromPositiveBalance_DecrementsAndRaisesEvent()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 3);
        inventory.ClearDomainEvents();

        inventory.Consume(amount: 1, now: SampleNow);

        inventory.Balance.Should().Be(2);
        var evt = AssertPublishedDomainEvent<PlayerUndoConsumedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.Amount.Should().Be(1);
        evt.RemainingBalance.Should().Be(2);
        evt.ConsumedOn.Should().Be(SampleNow);
    }

    [Test]
    public void Consume_DownToZero_LeavesBalanceAtZero()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 1);

        inventory.Consume(amount: 1, now: SampleNow);

        inventory.Balance.Should().Be(0);
    }

    [Test]
    public void Consume_ZeroAmount_BreaksRule()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        AssertBrokenRule<UndoAmountMustBePositiveRule>(() =>
            inventory.Consume(amount: 0, now: SampleNow));
    }

    [Test]
    public void Consume_NegativeAmount_BreaksRule()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        AssertBrokenRule<UndoAmountMustBePositiveRule>(() =>
            inventory.Consume(amount: -1, now: SampleNow));
    }

    [Test]
    public void Consume_MoreThanBalance_BreaksRule()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 2);

        AssertBrokenRule<UndoBalanceMustBeSufficientRule>(() =>
            inventory.Consume(amount: 3, now: SampleNow));
    }

    [Test]
    public void Consume_FromZeroBalance_BreaksRule()
    {
        var inventory = PlayerUndoInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        AssertBrokenRule<UndoBalanceMustBeSufficientRule>(() =>
            inventory.Consume(amount: 1, now: SampleNow));
    }
}
