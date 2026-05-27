using LexiLink.Common.Domain;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Events;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Rules;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

/// <summary>
/// A player's Diamond inventory: a persistent currency balance earned
/// through quests, admin grants, and future commerce flows.
/// Identified by <see cref="PlayerDiamondInventoryId"/> — same Guid as
/// the owning <c>PlayerId</c>, so cross-module references are by id
/// only.
///
/// Unlike Energy this aggregate has no max cap and no refill timer:
/// Diamonds are earned, not regenerated. Diamond is not a gameplay
/// invariant, so there is no Game sync gateway.
/// </summary>
public class PlayerDiamondInventory : Entity, IAggregateRoot
{
    public PlayerDiamondInventoryId Id { get; private set; }

    private int _balance;

    public int Balance => _balance;

    private PlayerDiamondInventory()
    {
        Id = null!;
    }

    private PlayerDiamondInventory(PlayerDiamondInventoryId id, int initialBalance)
    {
        Id = id;
        _balance = initialBalance;

        AddDomainEvent(new PlayerDiamondInventoryInitializedDomainEvent(Id.Value, _balance));
    }

    internal static PlayerDiamondInventory InitializeFor(Guid playerId, int initialBalance)
    {
        // Initial balance must be non-negative. Negative seeds are
        // nonsense; zero is the default (operator can opt in via
        // Diamond:InitialBalance config).
        CheckRule(new DiamondAmountMustBeNonNegativeRule(initialBalance));

        return new PlayerDiamondInventory(
            new PlayerDiamondInventoryId(playerId),
            initialBalance);
    }

    internal void Consume(int amount, DateTime now)
    {
        CheckRule(new DiamondAmountMustBePositiveRule(amount));
        CheckRule(new DiamondBalanceMustBeSufficientRule(_balance, amount));

        _balance -= amount;

        AddDomainEvent(new PlayerDiamondConsumedDomainEvent(Id.Value, amount, _balance, now));
    }

    internal void GrantBonus(int amount, DateTime now)
    {
        CheckRule(new DiamondAmountMustBePositiveRule(amount));

        // No cap — Diamond accumulation is rate-limited by earn paths,
        // not by an arbitrary maximum.
        _balance += amount;

        AddDomainEvent(new PlayerDiamondGrantedDomainEvent(Id.Value, amount, _balance, now));
    }

    /// <summary>
    /// Admin override: snap the balance to a specific value. Must
    /// be non-negative. Used by the admin console set endpoint.
    /// </summary>
    public void AdminSet(int newBalance, DateTime now)
    {
        CheckRule(new DiamondAmountMustBeNonNegativeRule(newBalance));

        _balance = newBalance;

        AddDomainEvent(new PlayerDiamondAdminSetDomainEvent(Id.Value, _balance, now));
    }

    /// <summary>
    /// Admin override: reset the balance to zero. Used by the admin
    /// console reset endpoint.
    /// </summary>
    public void AdminReset(DateTime now)
    {
        _balance = 0;

        AddDomainEvent(new PlayerDiamondAdminResetDomainEvent(Id.Value, now));
    }
}
