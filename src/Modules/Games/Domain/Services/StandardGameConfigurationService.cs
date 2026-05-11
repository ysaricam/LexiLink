using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Domain.Services;

public sealed class StandardGameConfigurationService : IGameConfigurationService
{
    public (int MinDepth, int MaxDepth) ResolveDepthRange(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => (3, 5),
        Difficulty.Medium => (5, 7),
        Difficulty.Hard => (7, 10),
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
    };

    public int ResolveMaxSteps(Difficulty difficulty, int targetDepth) => difficulty switch
    {
        Difficulty.Easy => targetDepth + 5,
        Difficulty.Medium => targetDepth + 4,
        Difficulty.Hard => targetDepth + 3,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
    };

    public int ResolveHints(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 3,
        Difficulty.Medium => 2,
        Difficulty.Hard => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
    };

    public int ResolveUndos(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 5,
        Difficulty.Medium => 3,
        Difficulty.Hard => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
    };

    public int ResolveResets(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 2,
        Difficulty.Medium => 1,
        Difficulty.Hard => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
    };
}
