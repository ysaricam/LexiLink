using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;

namespace LexiLink.Modules.Games.Domain.Games.Allowances;

public sealed class UndoAllowance : ValueObject
{
    public int Remaining { get; }
    public int Used { get; }

    private UndoAllowance() { }

    private UndoAllowance(int remaining, int used)
    {
        Remaining = remaining;
        Used = used;
    }

    public static UndoAllowance Of(int total) => new(total, 0);

    public UndoAllowance Consume()
    {
        CheckRule(new UndoAllowanceMustHaveRemainingRule(Remaining));
        return new UndoAllowance(Remaining - 1, Used + 1);
    }
}
