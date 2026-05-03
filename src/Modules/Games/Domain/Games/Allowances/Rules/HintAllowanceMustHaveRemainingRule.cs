using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Allowances.Rules;

public class HintAllowanceMustHaveRemainingRule : IBusinessRule
{
    private readonly int _remaining;

    public HintAllowanceMustHaveRemainingRule(int remaining)
    {
        _remaining = remaining;
    }

    public bool IsBroken() => _remaining <= 0;

    public string Message => "No hints remaining.";
}
