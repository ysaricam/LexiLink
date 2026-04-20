using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Score.Rules;

public sealed class ScoreCannotBeNegativeRule : IBusinessRule
{
    private readonly int _value;

    public ScoreCannotBeNegativeRule(int value)
    {
        _value = value;
    }

    public bool IsBroken() => _value < 0;

    public string Message => "Puan negatif olamaz.";
}
