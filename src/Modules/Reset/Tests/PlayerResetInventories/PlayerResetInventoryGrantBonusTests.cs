using LexiLink.Modules.Reset.Domain.PlayerResetInventories;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;
using LexiLink.Modules.Reset.Tests.SeedWork;

namespace LexiLink.Modules.Reset.Tests.PlayerResetInventories;

[TestFixture]
public class PlayerResetInventoryGrantBonusTests : TestBase
{
    private static readonly Guid SamplePlayerId = new("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime SampleNow = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void GrantBonus_FromZero_IncrementsAndRaisesEvent()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 0);
        inventory.ClearDomainEvents();

        inventory.GrantBonus(amount: 2, now: SampleNow);

        inventory.Balance.Should().Be(2);
        var evt = AssertPublishedDomainEvent<PlayerResetGrantedDomainEvent>(inventory);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.Amount.Should().Be(2);
        evt.NewBalance.Should().Be(2);
        evt.GrantedOn.Should().Be(SampleNow);
    }

    [Test]
    public void GrantBonus_NoMaximumCap_PermitsLargeBalances()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 100);

        inventory.GrantBonus(amount: 9_900, now: SampleNow);

        inventory.Balance.Should().Be(10_000);
    }

    [Test]
    public void GrantBonus_ZeroAmount_BreaksRule()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        AssertBrokenRule<ResetAmountMustBePositiveRule>(() =>
            inventory.GrantBonus(amount: 0, now: SampleNow));
    }

    [Test]
    public void GrantBonus_NegativeAmount_BreaksRule()
    {
        var inventory = PlayerResetInventory.InitializeFor(SamplePlayerId, initialBalance: 0);

        AssertBrokenRule<ResetAmountMustBePositiveRule>(() =>
            inventory.GrantBonus(amount: -1, now: SampleNow));
    }
}
