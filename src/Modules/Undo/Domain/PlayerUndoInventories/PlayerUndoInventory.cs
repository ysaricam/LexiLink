using LexiLink.Common.Domain;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Events;
using LexiLink.Modules.Undo.Domain.PlayerUndoInventories.Rules;

namespace LexiLink.Modules.Undo.Domain.PlayerUndoInventories;

/// <summary>
/// A player's undo inventory: a persistent counter of undo charges
/// earned through quest claims (or admin-granted in support cases).
/// Identified by <see cref="PlayerUndoInventoryId"/> — same Guid as
/// the owning <c>PlayerId</c>, so cross-module references are by id
/// only.
///
/// Unlike Energy this aggregate has no max cap and no refill timer:
/// undos are earned, not regenerated. Games no longer has a
/// per-game free undo quota; every in-game undo call will consume one
/// charge from this inventory through the IUndoGuard sync gateway.
/// </summary>
public class PlayerUndoInventory : Entity, IAggregateRoot
{
    public PlayerUndoInventoryId Id { get; private set; }

    private int _balance;

    public int Balance => _balance;

    private PlayerUndoInventory()
    {
        Id = null!;
    }

    private PlayerUndoInventory(PlayerUndoInventoryId id, int initialBalance)
    {
        Id = id;
        _balance = initialBalance;

        AddDomainEvent(new PlayerUndoInventoryInitializedDomainEvent(Id.Value, _balance));
    }

    internal static PlayerUndoInventory InitializeFor(Guid playerId, int initialBalance)
    {
        // Initial balance must be non-negative. Negative seeds are
        // nonsense; zero is the default (operator can opt in via
        // Undo:InitialBalance config).
        CheckRule(new UndoAmountMustBeNonNegativeRule(initialBalance));

        return new PlayerUndoInventory(
            new PlayerUndoInventoryId(playerId),
            initialBalance);
    }

    internal void Consume(int amount, DateTime now)
    {
        CheckRule(new UndoAmountMustBePositiveRule(amount));
        CheckRule(new UndoBalanceMustBeSufficientRule(_balance, amount));

        _balance -= amount;

        AddDomainEvent(new PlayerUndoConsumedDomainEvent(Id.Value, amount, _balance, now));
    }

    internal void GrantBonus(int amount, DateTime now)
    {
        CheckRule(new UndoAmountMustBePositiveRule(amount));

        // No cap — undo hoarding is rate-limited by quest cadence,
        // not by an arbitrary maximum.
        _balance += amount;

        AddDomainEvent(new PlayerUndoGrantedDomainEvent(Id.Value, amount, _balance, now));
    }

    /// <summary>
    /// Admin override: snap the balance to a specific value. Must
    /// be non-negative. Used by the admin console set endpoint.
    /// </summary>
    public void AdminSet(int newBalance, DateTime now)
    {
        CheckRule(new UndoAmountMustBeNonNegativeRule(newBalance));

        _balance = newBalance;

        AddDomainEvent(new PlayerUndoAdminSetDomainEvent(Id.Value, _balance, now));
    }

    /// <summary>
    /// Admin override: reset the balance to zero. Used by the admin
    /// console reset endpoint.
    /// </summary>
    public void AdminReset(DateTime now)
    {
        _balance = 0;

        AddDomainEvent(new PlayerUndoAdminResetDomainEvent(Id.Value, now));
    }
}
