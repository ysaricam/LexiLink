using LexiLink.Common.Domain;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Events;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories;

/// <summary>
/// A player's reset inventory: a persistent counter of reset charges
/// earned through quest claims (or admin-granted in support cases).
/// Identified by <see cref="PlayerResetInventoryId"/> — same Guid as
/// the owning <c>PlayerId</c>, so cross-module references are by id
/// only.
///
/// Unlike Energy this aggregate has no max cap and no refill timer:
/// resets are earned, not regenerated. Games no longer has a
/// per-game free reset quota; every in-game reset call will consume one
/// charge from this inventory through the IResetGuard sync gateway.
/// </summary>
public class PlayerResetInventory : Entity, IAggregateRoot
{
    public PlayerResetInventoryId Id { get; private set; }

    private int _balance;

    public int Balance => _balance;

    private PlayerResetInventory()
    {
        Id = null!;
    }

    private PlayerResetInventory(PlayerResetInventoryId id, int initialBalance)
    {
        Id = id;
        _balance = initialBalance;

        AddDomainEvent(new PlayerResetInventoryInitializedDomainEvent(Id.Value, _balance));
    }

    internal static PlayerResetInventory InitializeFor(Guid playerId, int initialBalance)
    {
        // Initial balance must be non-negative. Negative seeds are
        // nonsense; zero is the default (operator can opt in via
        // Reset:InitialBalance config).
        CheckRule(new ResetAmountMustBeNonNegativeRule(initialBalance));

        return new PlayerResetInventory(
            new PlayerResetInventoryId(playerId),
            initialBalance);
    }

    internal void Consume(int amount, DateTime now)
    {
        CheckRule(new ResetAmountMustBePositiveRule(amount));
        CheckRule(new ResetBalanceMustBeSufficientRule(_balance, amount));

        _balance -= amount;

        AddDomainEvent(new PlayerResetConsumedDomainEvent(Id.Value, amount, _balance, now));
    }

    internal void GrantBonus(int amount, DateTime now)
    {
        CheckRule(new ResetAmountMustBePositiveRule(amount));

        // No cap — reset hoarding is rate-limited by quest cadence,
        // not by an arbitrary maximum.
        _balance += amount;

        AddDomainEvent(new PlayerResetGrantedDomainEvent(Id.Value, amount, _balance, now));
    }

    /// <summary>
    /// Admin override: snap the balance to a specific value. Must
    /// be non-negative. Used by the admin console set endpoint.
    /// </summary>
    public void AdminSet(int newBalance, DateTime now)
    {
        CheckRule(new ResetAmountMustBeNonNegativeRule(newBalance));

        _balance = newBalance;

        AddDomainEvent(new PlayerResetAdminSetDomainEvent(Id.Value, _balance, now));
    }

    /// <summary>
    /// Admin override: reset the balance to zero. Used by the admin
    /// console reset endpoint.
    /// </summary>
    public void AdminReset(DateTime now)
    {
        _balance = 0;

        AddDomainEvent(new PlayerResetAdminResetDomainEvent(Id.Value, now));
    }
}
