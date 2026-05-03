using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Domain.Services;

public interface IScoreCalculator
{
    Score Calculate(
        Difficulty difficulty,
        int targetDepth,
        int stepsTaken,
        int hintsUsed,
        int undosUsed,
        int resetsUsed);
}
