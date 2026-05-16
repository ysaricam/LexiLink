using LexiLink.Common.Domain;

namespace LexiLink.Modules.Energy.Domain.PlayerEnergies.Rules;

public class EnergyAmountCannotBeNegativeRule : IBusinessRule
{
    private readonly int _amount;

    public EnergyAmountCannotBeNegativeRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount < 0;

    public string Message => "Energy amount cannot be negative.";
}
