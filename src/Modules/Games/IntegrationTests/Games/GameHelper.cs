using LexiLink.Modules.Games.Application.Games.CreateGame;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.IntegrationTests.Categories;
using LexiLink.Modules.Games.IntegrationTests.Links;
using MediatR;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

internal sealed record GameSetup(
    Guid CategoryId,
    Guid PlayerId,
    Guid GameId,
    Dictionary<string, Guid> LinksByValue);

internal static class GameHelper
{
    public static Task<Guid> CreateGameAsync(
        ISender sender,
        Guid categoryId,
        Guid? playerId = null,
        Difficulty difficulty = Difficulty.Easy)
        => sender.Send(new CreateGameCommand(playerId ?? Guid.NewGuid(), categoryId, difficulty));

    /// <summary>
    /// Builds: 1 Category, 6 Links bidirectionally chained (cat↔mat↔bat↔bag↔bug↔rug),
    /// then a Game using the supplied difficulty.
    /// </summary>
    public static async Task<GameSetup> SetupChainedGameAsync(ISender sender, Difficulty difficulty = Difficulty.Easy)
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(sender);
        var words = new[] { "cat", "mat", "bat", "bag", "bug", "rug" };
        var ids = await LinkHelper.CreateLinksAsync(sender, categoryId, words);
        for (var i = 0; i < ids.Count - 1; i++)
        {
            await LinkHelper.LinkBidirectionallyAsync(sender, ids[i], ids[i + 1]);
        }
        var playerId = Guid.NewGuid();
        var gameId = await CreateGameAsync(sender, categoryId, playerId, difficulty);

        return new GameSetup(
            categoryId,
            playerId,
            gameId,
            words.Zip(ids).ToDictionary(t => t.First, t => t.Second));
    }
}
