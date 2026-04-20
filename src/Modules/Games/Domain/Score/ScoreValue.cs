using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Score.Rules;

namespace LexiLink.Modules.Games.Domain.Score;

public sealed class ScoreValue : ValueObject
{
    public int Value { get; }

    private ScoreValue(int value)
    {
        CheckRule(new ScoreCannotBeNegativeRule(value));
        Value = value;
    }

    public static ScoreValue Of(int value)
    {
        return new ScoreValue(value);
    }

    public static ScoreValue Zero => new ScoreValue(0);

    public ScoreValue Add(int amount)
    {
        return new ScoreValue(Value + amount);
    }

    public override string ToString() => Value.ToString();
}
