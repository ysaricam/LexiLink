using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Score;
using LexiLink.Modules.Games.Domain.Score.Rules;

namespace LexiLink.Tests.Games.Domain.Score;

public class ScoreValueTests
{
    [Fact]
    public void Of_ShouldSetPropertyValue()
    {
        var score = ScoreValue.Of(100);
        Assert.Equal(100, score.Value);
    }

    [Fact]
    public void Of_ShouldThrow_WhenValueIsNegative()
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => ScoreValue.Of(-1));
        Assert.IsType<ScoreCannotBeNegativeRule>(exception.BrokenRule);
    }

    [Fact]
    public void Zero_ShouldBeZero()
    {
        Assert.Equal(0, ScoreValue.Zero.Value);
    }

    [Fact]
    public void Add_ShouldReturnNewScoreWithValueAdded()
    {
        var score = ScoreValue.Of(100);
        var newScore = score.Add(50);
        
        Assert.Equal(150, newScore.Value);
        Assert.NotSame(score, newScore); // Value object should be immutable
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameValue()
    {
        var a = ScoreValue.Of(100);
        var b = ScoreValue.Of(100);
        Assert.Equal(a, b);
    }
}
