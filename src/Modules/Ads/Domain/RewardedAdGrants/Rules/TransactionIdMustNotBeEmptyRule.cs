using LexiLink.Common.Domain;

namespace LexiLink.Modules.Ads.Domain.RewardedAdGrants.Rules;

public class TransactionIdMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _transactionId;

    public TransactionIdMustNotBeEmptyRule(string? transactionId)
    {
        _transactionId = transactionId;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_transactionId);

    public string Message => "Rewarded-ad transaction id must not be empty.";
}
