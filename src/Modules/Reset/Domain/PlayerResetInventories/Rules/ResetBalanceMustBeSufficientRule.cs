using LexiLink.Common.Domain;

namespace LexiLink.Modules.Reset.Domain.PlayerResetInventories.Rules;

public class ResetBalanceMustBeSufficientRule : IBusinessRule
{
    private readonly int _currentBalance;
    private readonly int _requestedAmount;

    public ResetBalanceMustBeSufficientRule(int currentBalance, int requestedAmount)
    {
        _currentBalance = currentBalance;
        _requestedAmount = requestedAmount;
    }

    public bool IsBroken() => _currentBalance < _requestedAmount;

    public string Message =>
        $"Reset balance is insufficient: {_currentBalance} available, {_requestedAmount} required.";
}
