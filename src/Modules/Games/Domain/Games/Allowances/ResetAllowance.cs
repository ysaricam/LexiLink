using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;

namespace LexiLink.Modules.Games.Domain.Games.Allowances;

public sealed class ResetAllowance : ValueObject
{
    public int Remaining { get; }
    public int Used { get; }

    private ResetAllowance() { }

    private ResetAllowance(int remaining, int used)
    {
        Remaining = remaining;
        Used = used;
    }

    public static ResetAllowance Of(int total) => new(total, 0);

    public ResetAllowance Consume()
    {
        CheckRule(new ResetAllowanceMustHaveRemainingRule(Remaining));
        return new ResetAllowance(Remaining - 1, Used + 1);
    }
}
