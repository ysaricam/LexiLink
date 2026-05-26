using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Domain.Services;

public interface IGameConfigurationService
{
    (int MinDepth, int MaxDepth) ResolveDepthRange(Difficulty difficulty);
    int ResolveMaxSteps(Difficulty difficulty, int targetDepth);
    int ResolveHints(Difficulty difficulty);
}
