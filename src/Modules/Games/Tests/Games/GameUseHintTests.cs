using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Games.Rules;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameUseHintTests : GameTestsBase
{
    [Test]
    public void UseHint_FromStartPosition_ReturnsOnWrongPathWithFirstOptimalStep()
    {
        // Player is at start; start is NOT part of the optimal path,
        // so the hint reports OnWrongPath but recommends the first step
        // toward target via live BFS through the neighbor resolver.
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [n1, target]);
        var game = BuildGame(puzzle, hints: 2);
        game.Start();
        var resolver = NeighborResolver((start, [n1]), (n1, [target]));

        var hint = game.UseHint(resolver);

        hint.Type.Should().Be(HintType.OnWrongPath);
        hint.RecommendedLinkId.Should().Be(n1);
        AssertPublishedDomainEvent<HintUsedDomainEvent>(game)
            .HintResult.RecommendedLinkId.Should().Be(n1);
    }

    [Test]
    public void UseHint_WhenStandingOnOptimalPath_ReturnsOnCorrectPathWithNextStep()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var n2 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [n1, n2, target]);
        var game = BuildGame(puzzle, maxSteps: 5, hints: 2);
        game.Start();
        var resolver = NeighborResolver(
            (start, [n1]),
            (n1, [n2]),
            (n2, [target]));
        // Step onto the path at n1.
        game.MakeStep(n1, resolver, FixedScoreCalculator());

        var hint = game.UseHint(resolver);

        hint.Type.Should().Be(HintType.OnCorrectPath);
        hint.RecommendedLinkId.Should().Be(n2);
    }

    [Test]
    public void UseHint_WhenNotStarted_BreaksGameMustBeInProgressRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle, hints: 2);
        var resolver = NeighborResolver();

        AssertBrokenRule<GameMustBeInProgressRule>(() => game.UseHint(resolver));
    }

    [Test]
    public void UseHint_WhenNoHintsRemaining_BreaksHintAllowanceMustHaveRemainingRule()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, hints: 1);
        game.Start();
        var resolver = NeighborResolver((start, [target]));
        game.UseHint(resolver);

        AssertBrokenRule<HintAllowanceMustHaveRemainingRule>(() => game.UseHint(resolver));
    }
}
