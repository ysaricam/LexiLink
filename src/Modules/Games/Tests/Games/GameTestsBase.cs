using System.Reflection;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;
using LexiLink.Modules.Games.Tests.SeedWork;
using NSubstitute;

namespace LexiLink.Modules.Games.Tests.Games;

public abstract class GameTestsBase : TestBase
{
    protected static GameId NewGameId() => new(Guid.NewGuid());
    protected static LinkId NewLinkId() => new(Guid.NewGuid());
    protected static CategoryId NewCategoryId() => new(Guid.NewGuid());
    protected static Guid NewPlayerId() => Guid.NewGuid();

    /// <summary>
    /// Builds a Puzzle directly via its private constructor — bypasses random start-link
    /// selection so tests can pin start/target/optimalPath deterministically.
    /// </summary>
    protected static Puzzle BuildPuzzle(
        LinkId startLinkId,
        LinkId targetLinkId,
        IEnumerable<LinkId> optimalPath,
        Difficulty difficulty = Difficulty.Easy,
        CategoryId? categoryId = null)
    {
        var ctor = typeof(Puzzle).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(CategoryId), typeof(Difficulty), typeof(LinkId), typeof(LinkId), typeof(IEnumerable<LinkId>)],
            modifiers: null)
            ?? throw new InvalidOperationException("Puzzle private ctor signature changed.");
        return (Puzzle)ctor.Invoke([categoryId ?? NewCategoryId(), difficulty, startLinkId, targetLinkId, optimalPath]);
    }

    protected static Game BuildGame(
        Puzzle puzzle,
        int maxSteps = 10,
        int hints = 3,
        int undos = 5,
        int resets = 2,
        Guid? playerId = null,
        bool clearEvents = true)
    {
        var game = Game.Create(playerId ?? NewPlayerId(), puzzle, maxSteps, hints, undos, resets);
        if (clearEvents) DomainEventsTestHelper.ClearAllDomainEvents(game);
        return game;
    }

    protected static ILinkNeighborResolver NeighborResolver(params (LinkId From, IReadOnlyCollection<LinkId> To)[] mappings)
    {
        var resolver = Substitute.For<ILinkNeighborResolver>();
        foreach (var (from, to) in mappings)
        {
            resolver.GetOutgoingLinkIds(from).Returns(to);
        }
        return resolver;
    }

    /// <summary>Convenience: linear chain a→b→c→…  Each node's only neighbor is the next one.</summary>
    protected static ILinkNeighborResolver LinearNeighborResolver(params LinkId[] chain)
    {
        var resolver = Substitute.For<ILinkNeighborResolver>();
        for (var i = 0; i < chain.Length - 1; i++)
        {
            resolver.GetOutgoingLinkIds(chain[i]).Returns(new List<LinkId> { chain[i + 1] });
        }
        resolver.GetOutgoingLinkIds(chain[^1]).Returns(new List<LinkId>());
        return resolver;
    }

    protected static IScoreCalculator FixedScoreCalculator(int points = 100)
    {
        var calc = Substitute.For<IScoreCalculator>();
        calc.Calculate(Arg.Any<Difficulty>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(Score.Of(points));
        return calc;
    }
}
