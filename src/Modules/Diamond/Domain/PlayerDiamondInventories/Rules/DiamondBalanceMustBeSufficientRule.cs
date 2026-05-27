using LexiLink.Common.Domain;

namespace LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories.Rules;

public class DiamondBalanceMustBeSufficientRule : IBusinessRule
{
    private readonly int _currentBalance;
    private readonly int _requestedAmount;

    public DiamondBalanceMustBeSufficientRule(int currentBalance, int requestedAmount)
    {
        _currentBalance = currentBalance;
        _requestedAmount = requestedAmount;
    }

    public bool IsBroken() => _currentBalance < _requestedAmount;

    public string Message =>
        $"Diamond balance is insufficient: {_currentBalance} available, {_requestedAmount} required.";
}
