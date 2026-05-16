using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

public class EnergyAmountCannotExceedMaximumRule : IBusinessRule
{
    private readonly int _amount;
    private readonly int _maximumAmount;

    public EnergyAmountCannotExceedMaximumRule(int amount, int maximumAmount)
    {
        _amount = amount;
        _maximumAmount = maximumAmount;
    }

    public bool IsBroken() => _amount > _maximumAmount;

    public string Message => $"Energy amount cannot exceed the configured maximum of {_maximumAmount}.";
}
