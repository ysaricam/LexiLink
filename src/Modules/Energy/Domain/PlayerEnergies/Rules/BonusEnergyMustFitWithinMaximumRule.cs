using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

public class BonusEnergyMustFitWithinMaximumRule : IBusinessRule
{
    private readonly int _currentAmount;
    private readonly int _maximumAmount;
    private readonly int _bonusAmount;

    public BonusEnergyMustFitWithinMaximumRule(
        int currentAmount,
        int maximumAmount,
        int bonusAmount)
    {
        _currentAmount = currentAmount;
        _maximumAmount = maximumAmount;
        _bonusAmount = bonusAmount;
    }

    public bool IsBroken() => _currentAmount + _bonusAmount > _maximumAmount;

    public string Message => "Bonus energy cannot exceed the player's maximum energy.";
}
