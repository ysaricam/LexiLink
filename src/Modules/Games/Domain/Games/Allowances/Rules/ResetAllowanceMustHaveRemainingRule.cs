using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Allowances.Rules;

public class ResetAllowanceMustHaveRemainingRule : IBusinessRule
{
    private readonly int _remaining;

    public ResetAllowanceMustHaveRemainingRule(int remaining)
    {
        _remaining = remaining;
    }

    public bool IsBroken() => _remaining <= 0;

    public string Message => "No resets remaining.";
}
