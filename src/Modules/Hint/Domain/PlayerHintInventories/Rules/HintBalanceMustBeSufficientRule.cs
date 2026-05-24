using LexiLink.Common.Domain;

namespace LexiLink.Modules.Hint.Domain.PlayerHintInventories.Rules;

public class HintBalanceMustBeSufficientRule : IBusinessRule
{
    private readonly int _currentBalance;
    private readonly int _requestedAmount;

    public HintBalanceMustBeSufficientRule(int currentBalance, int requestedAmount)
    {
        _currentBalance = currentBalance;
        _requestedAmount = requestedAmount;
    }

    public bool IsBroken() => _currentBalance < _requestedAmount;

    public string Message =>
        $"Hint balance is insufficient: {_currentBalance} available, {_requestedAmount} required.";
}
