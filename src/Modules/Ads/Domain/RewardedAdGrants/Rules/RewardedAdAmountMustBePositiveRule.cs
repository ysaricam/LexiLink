using LexiLink.Common.Domain;

namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants.Rules;

public class RewardedAdAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _amount;

    public RewardedAdAmountMustBePositiveRule(int amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => "Rewarded-ad Diamond amount must be positive.";
}
