using LexiLink.Modules.Hint.Domain.PlayerHintInventories;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;
using LexiLink.Modules.Hint.Tests.SeedWork;

namespace LexiLink.Modules.Hint.Tests.PlayerHintInventories;

[TestFixture]
public class PlayerHintInventoryConsumeTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SampleNow = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Consume_FromPositiveBalance_DecrementsAndRaisesEvent()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 3);
        inventory.ClearDomainEvents();

        inventory.Consume(amount: 1, now: SampleNow);

        inventory.Balance.Should().Be(2);
        var evt = AssertPublishedDomainEvent<PlayerHintConsumedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.Amount.Should().Be(1);
        evt.RemainingBalance.Should().Be(2);
        evt.ConsumedOn.Should().Be(SampleNow);
    }

    [Test]
    public void Consume_DownToZero_LeavesBalanceAtZero()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 1);

        inventory.Consume(amount: 1, now: SampleNow);

        inventory.Balance.Should().Be(0);
    }

    [Test]
    public void Consume_ZeroAmount_BreaksRule()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        AssertBrokenRule<HintAmountMustBePositiveRule>(() =>
            inventory.Consume(amount: 0, now: SampleNow));
    }

    [Test]
    public void Consume_NegativeAmount_BreaksRule()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 3);

        AssertBrokenRule<HintAmountMustBePositiveRule>(() =>
            inventory.Consume(amount: -1, now: SampleNow));
    }

    [Test]
    public void Consume_MoreThanBalance_BreaksRule()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 2);

        AssertBrokenRule<HintBalanceMustBeSufficientRule>(() =>
            inventory.Consume(amount: 3, now: SampleNow));
    }

    [Test]
    public void Consume_FromZeroBalance_BreaksRule()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        AssertBrokenRule<HintBalanceMustBeSufficientRule>(() =>
            inventory.Consume(amount: 1, now: SampleNow));
    }
}
