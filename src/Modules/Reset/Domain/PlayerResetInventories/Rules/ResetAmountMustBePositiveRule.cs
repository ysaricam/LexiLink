using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;

public class ResetAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;

    public ResetAmountMustBePositiveRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => "Reset amount must be positive.";
}
