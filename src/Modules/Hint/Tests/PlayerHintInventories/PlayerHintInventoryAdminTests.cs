using LexiLink.Modules.Hint.Domain.PlayerHintInventories;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;
using LexiLink.Modules.Hint.Tests.SeedWork;

namespace LexiLink.Modules.Hint.Tests.PlayerHintInventories;

[TestFixture]
public class PlayerHintInventoryAdminTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SampleNow = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void AdminSet_OverridesBalanceToExactValue()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 5);
        inventory.ClearDomainEvents();

        inventory.AdminSet(newBalance: 12, now: SampleNow);

        inventory.Balance.Should().Be(12);
        var evt = AssertPublishedDomainEvent<PlayerHintAdminSetDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.NewBalance.Should().Be(12);
        evt.SetOn.Should().Be(SampleNow);
    }

    [Test]
    public void AdminSet_ToZero_IsAllowed()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 5);

        inventory.AdminSet(newBalance: 0, now: SampleNow);

        inventory.Balance.Should().Be(0);
    }

    [Test]
    public void AdminSet_Negative_BreaksRule()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 5);

        AssertBrokenRule<HintAmountMustBeNonNegativeRule>(() =>
            inventory.AdminSet(newBalance: -1, now: SampleNow));
    }

    [Test]
    public void AdminReset_SnapsBalanceToZeroAndRaisesEvent()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 7);
        inventory.ClearDomainEvents();

        inventory.AdminReset(now: SampleNow);

        inventory.Balance.Should().Be(0);
        var evt = AssertPublishedDomainEvent<PlayerHintAdminResetDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.ResetOn.Should().Be(SampleNow);
    }

    [Test]
    public void AdminReset_FromZero_StaysAtZero()
    {
        var inventory = PlayerHintInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.AdminReset(now: SampleNow);

        inventory.Balance.Should().Be(0);
    }
}
