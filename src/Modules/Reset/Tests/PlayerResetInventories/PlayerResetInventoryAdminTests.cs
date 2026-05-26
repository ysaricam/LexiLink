using LexiLink.Modules.Reset.Domain.PlayerResetInventories;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;
using LexiLink.Modules.Reset.Tests.SeedWork;

namespace LexiLink.Modules.Reset.Tests.PlayerResetInventories;

[TestFixture]
public class PlayerResetInventoryAdminTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SampleNow = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void AdminSet_OverridesBalanceToExactValue()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 5);
        inventory.ClearDomainEvents();

        inventory.AdminSet(newBalance: 12, now: SampleNow);

        inventory.Balance.Should().Be(12);
        var evt = AssertPublishedDomainEvent<PlayerResetAdminSetDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.NewBalance.Should().Be(12);
        evt.SetOn.Should().Be(SampleNow);
    }

    [Test]
    public void AdminSet_ToZero_IsAllowed()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 5);

        inventory.AdminSet(newBalance: 0, now: SampleNow);

        inventory.Balance.Should().Be(0);
    }

    [Test]
    public void AdminSet_Negative_BreaksRule()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 5);

        AssertBrokenRule<ResetAmountMustBeNonNegativeRule>(() =>
            inventory.AdminSet(newBalance: -1, now: SampleNow));
    }

    [Test]
    public void AdminReset_SnapsBalanceToZeroAndRaisesEvent()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 7);
        inventory.ClearDomainEvents();

        inventory.AdminReset(now: SampleNow);

        inventory.Balance.Should().Be(0);
        var evt = AssertPublishedDomainEvent<PlayerResetAdminResetDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.ResetOn.Should().Be(SampleNow);
    }

    [Test]
    public void AdminReset_FromZero_StaysAtZero()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        inventory.AdminReset(now: SampleNow);

        inventory.Balance.Should().Be(0);
    }
}
