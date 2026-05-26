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

    // Sprint H decision (locked): every game ships with exactly one
    // free hint regardless of difficulty. Players who want more spend
    // from their persistent PlayerHintInventory via the IHintGuard
    // sync gateway.
    public int ResolveHints(Difficulty difficulty) => 1;
}
