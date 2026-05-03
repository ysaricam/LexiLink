using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games;

public sealed class StepBudget : ValueObject
{
    public int Max { get; }
    public int Taken { get; }

    public int Remaining => Max - Taken;
    public bool IsExhausted => Taken >= Max;
    public bool IsAtLastWarning => Taken == Max - 1;
    public bool IsBelowLastWarning => Taken < Max - 1;

    private StepBudget() { }

    private StepBudget(int max, int taken)
    {
        Max = max;
        Taken = taken;
    }

    public static StepBudget Of(int max) => new(max, 0);

    public StepBudget Step() => new(Max, Taken + 1);

    public StepBudget UndoStep() => new(Max, Math.Max(0, Taken - 1));

    public StepBudget Reset() => new(Max, 0);
}
