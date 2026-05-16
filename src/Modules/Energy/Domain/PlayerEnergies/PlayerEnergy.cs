using LexiLink.Common.Domain;
using LexiLink.Modules.Energy.Domain.PlayerEnergies.Events;
using LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies;

public class PlayerEnergy : Entity, IAggregateRoot
{
    public PlayerEnergyId Id { get; private set; }

    private int _currentAmount;
    private int _maximumAmount;
    private int _rechargeIntervalSeconds;
    private DateTime _lastRefilledOn;

    public int CurrentAmount => _currentAmount;
    public int MaximumAmount => _maximumAmount;
    public int RechargeIntervalSeconds => _rechargeIntervalSeconds;
    public DateTime LastRefilledOn => _lastRefilledOn;

    private PlayerEnergy()
    {
        Id = null!;
    }

    private PlayerEnergy(
        PlayerEnergyId id,
        int maximumAmount,
        int rechargeIntervalSeconds,
        DateTime initializedAt)
    {
        CheckRule(new EnergyConfigurationMustBeValidRule(maximumAmount, rechargeIntervalSeconds));

        Id = id;
        _maximumAmount = maximumAmount;
        _rechargeIntervalSeconds = rechargeIntervalSeconds;
        _currentAmount = maximumAmount;
        _lastRefilledOn = initializedAt;
    }

    internal static PlayerEnergy InitializeFor(
        Guid playerId,
        int maximumAmount,
        int rechargeIntervalSeconds,
        DateTime initializedAt)
    {
        return new PlayerEnergy(
            new PlayerEnergyId(playerId),
            maximumAmount,
            rechargeIntervalSeconds,
            initializedAt);
    }

    internal void RechargeBasedOnElapsedTime(DateTime now)
    {
        var projection = EnergyRefillCalculator.Project(
            _currentAmount,
            _maximumAmount,
            _lastRefilledOn,
            _rechargeIntervalSeconds,
            now);

        var gained = projection.CurrentAmount - _currentAmount;
        if (gained <= 0)
        {
            return;
        }

        _currentAmount = projection.CurrentAmount;
        _lastRefilledOn = projection.LastRefilledOn;

        AddDomainEvent(new PlayerEnergyRefilledDomainEvent(Id.Value, gained, _currentAmount));
    }

    internal void Consume(int amount, DateTime now)
    {
        CheckRule(new EnergyAmountCannotBeNegativeRule(amount));

        RechargeBasedOnElapsedTime(now);

        CheckRule(new EnergyMustBeSufficientToConsumeRule(_currentAmount, amount));

        var wasAtOrAboveMaximum = _currentAmount >= _maximumAmount;

        _currentAmount -= amount;

        // Start the recharge cycle only when consume actually drops the bucket
        // *below* max. Consuming over-max (10/5 → 9/5) or down-to-max (6/5 → 5/5)
        // must leave the timer idle so the next refill doesn't tick prematurely.
        if (wasAtOrAboveMaximum && _currentAmount < _maximumAmount)
        {
            _lastRefilledOn = now;
        }

        AddDomainEvent(new PlayerEnergyConsumedDomainEvent(Id.Value, amount, _currentAmount));
    }

    internal void GrantBonus(int amount, DateTime now)
    {
        CheckRule(new BonusAmountMustBePositiveRule(amount));

        // Bonus may push current above max; this is intentional. The recharge
        // calculator already short-circuits when current >= max, so the timer
        // stays idle while the over-max balance is drained back down.
        _currentAmount += amount;
    }
}
