using LexiLink.Modules.Games.Domain.Score;

namespace LexiLink.Tests.Games.Domain.Score;

public class ScoreCalculatorTests
{
    private readonly ScoreCalculator _calculator = new ScoreCalculator();

    [Fact]
    public void Calculate_ShouldReturnZero_WhenStepsIsZeroOrLess()
    {
        var result = _calculator.Calculate(3, 0, 10, 0);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Calculate_ShouldReturnBasePlusBonus_WhenValid()
    {
        // targetDepth = 3 (300 base)
        // currentSteps = 3
        // maxSteps = 10
        // comboCount = 0
        // bonus = (10-3)*20 = 140
        // total = 300 + 140 = 440
        var result = _calculator.Calculate(3, 3, 10, 0);
        Assert.Equal(440, result.Value);
    }

    [Fact]
    public void Calculate_ShouldIncludeEfficiencyBonus_WhenStepsLessThanDepth()
    {
        // targetDepth = 3 (300 base)
        // currentSteps = 2
        // maxSteps = 10
        // comboCount = 0
        // bonus = (10-2)*20 = 160
        // efficiency = (3-2)*50 = 50
        // total = 300 + 160 + 50 = 510
        var result = _calculator.Calculate(3, 2, 10, 0);
        Assert.Equal(510, result.Value);
    }

    [Fact]
    public void Calculate_ShouldIncludeComboBonus_WhenComboCountIsGreaterThanZero()
    {
        // targetDepth = 3 (300 base)
        // currentSteps = 3
        // maxSteps = 10
        // comboCount = 3 (3 * 50 = 150 combo bonus)
        // remaining bonus = (10-3)*20 = 140
        // total = 300 + 140 + 150 = 590
        var result = _calculator.Calculate(3, 3, 10, 3);
        Assert.Equal(590, result.Value);
    }
}
