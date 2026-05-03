using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Domain.Services;

public sealed class StandardScoreCalculator : IScoreCalculator
{
    private const int BaseMultiplier = 100;
    private const int StepOverPenalty = 10;
    private const int HintPenalty = 20;
    private const int UndoPenalty = 15;
    private const int ResetPenalty = 40;

    public Score Calculate(
        Difficulty difficulty,
        int targetDepth,
        int stepsTaken,
        int hintsUsed,
        int undosUsed,
        int resetsUsed)
    {
        var baseScore = targetDepth * BaseMultiplier;

        var totalPenalty =
            Math.Max(0, stepsTaken - targetDepth) * StepOverPenalty
            + hintsUsed * HintPenalty
            + undosUsed * UndoPenalty
            + resetsUsed * ResetPenalty;

        var rawScore = Math.Max(0, baseScore - totalPenalty);
        var finalScore = (int)Math.Round(rawScore * GetDifficultyMultiplier(difficulty));

        return Score.Of(finalScore);
    }

    private static double GetDifficultyMultiplier(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 1.00,
        Difficulty.Medium => 1.15,
        Difficulty.Hard => 1.25,
        _ => 1.00
    };
}
