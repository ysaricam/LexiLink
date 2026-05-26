using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;

public class ResetAmountMustBeNonNegativeRule : IBusinessRule
{
    private readonly int _amount;

    public ResetAmountMustBeNonNegativeRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount < 0;

    public string Message => "Reset amount cannot be negative.";
}
