using LexiLink.Common.Domain;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Events;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories;

/// <summary>
/// A player's hint inventory: a persistent counter of hint charges
/// earned through quest claims (or admin-granted in support cases).
/// Identified by <see cref="PlayerHintInventoryId"/> — same Guid as
/// the owning <c>PlayerId</c>, so cross-module references are by id
/// only.
///
/// Unlike Energy this aggregate has no max cap and no refill timer:
/// hints are earned, not regenerated. The per-game free hint (1 fixed
/// across all difficulties) lives on the <c>Game</c> aggregate's
/// <c>HintAllowance</c> VO and is consumed first; when that runs out
/// the call falls through to <c>Consume</c> here via the
/// <c>IHintGuard</c> sync gateway.
/// </summary>
public class PlayerHintInventory : Entity, IAggregateRoot
{
    public PlayerHintInventoryId Id { get; private set; }

    private int _balance;

    public int Balance => _balance;

    private PlayerHintInventory()
    {
        Id = null!;
    }

    private PlayerHintInventory(PlayerHintInventoryId id, int initialBalance)
    {
        Id = id;
        _balance = initialBalance;

        AddDomainEvent(new PlayerHintInventoryInitializedDomainEvent(Id.Value, _balance));
    }

    internal static PlayerHintInventory InitializeFor(Guid playerId, int initialBalance)
    {
        // Initial balance must be non-negative. Negative seeds are
        // nonsense; zero is the default (operator can opt in via
        // Hint:InitialBalance config).
        CheckRule(new HintAmountMustBeNonNegativeRule(initialBalance));

        return new PlayerHintInventory(
            new PlayerHintInventoryId(playerId),
            initialBalance);
    }

    internal void Consume(int amount, DateTime now)
    {
        CheckRule(new HintAmountMustBePositiveRule(amount));
        CheckRule(new HintBalanceMustBeSufficientRule(_balance, amount));

        _balance -= amount;

        AddDomainEvent(new PlayerHintConsumedDomainEvent(Id.Value, amount, _balance, now));
    }

    internal void GrantBonus(int amount, DateTime now)
    {
        CheckRule(new HintAmountMustBePositiveRule(amount));

        // No cap — hint hoarding is rate-limited by quest cadence,
        // not by an arbitrary maximum.
        _balance += amount;

        AddDomainEvent(new PlayerHintGrantedDomainEvent(Id.Value, amount, _balance, now));
    }

    /// <summary>
    /// Admin override: snap the balance to a specific value. Must
    /// be non-negative. Used by the admin console set endpoint.
    /// </summary>
    public void AdminSet(int newBalance, DateTime now)
    {
        CheckRule(new HintAmountMustBeNonNegativeRule(newBalance));

        _balance = newBalance;

        AddDomainEvent(new PlayerHintAdminSetDomainEvent(Id.Value, _balance, now));
    }

    /// <summary>
    /// Admin override: reset the balance to zero. Used by the admin
    /// console reset endpoint.
    /// </summary>
    public void AdminReset(DateTime now)
    {
        _balance = 0;

        AddDomainEvent(new PlayerHintAdminResetDomainEvent(Id.Value, now));
    }
}
