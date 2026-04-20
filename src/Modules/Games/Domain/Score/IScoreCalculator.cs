using LexiLink.Modules.Games.Domain.Score;

namespace LexiLink.Modules.Games.Domain.Score;

public interface IScoreCalculator
{
    ScoreValue Calculate(int targetDepth, int currentSteps, int maxSteps, int comboCount);
}
