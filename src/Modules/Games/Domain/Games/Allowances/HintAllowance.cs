using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;

namespace LexiLink.Modules.Games.Domain.Games.Allowances;

public sealed class HintAllowance : ValueObject
{
    public int Remaining { get; }
    public int Used { get; }

    private HintAllowance() { }

    private HintAllowance(int remaining, int used)
    {
        Remaining = remaining;
        Used = used;
    }

    public static HintAllowance Of(int total) => new(total, 0);

    public HintAllowance Consume()
    {
        CheckRule(new HintAllowanceMustHaveRemainingRule(Remaining));
        return new HintAllowance(Remaining - 1, Used + 1);
    }
}
