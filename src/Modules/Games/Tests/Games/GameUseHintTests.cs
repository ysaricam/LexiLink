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
        // so the hint reports OnWrongPath but recommends the first step.
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [n1, target]);
        var game = BuildGame(puzzle, hints: 2);
        game.Start();

        var hint = game.UseHint();

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
        // Step onto the path at n1.
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());

        var hint = game.UseHint();

        hint.Type.Should().Be(HintType.OnCorrectPath);
        hint.RecommendedLinkId.Should().Be(n2);
    }

    [Test]
    public void UseHint_WhenNotStarted_BreaksGameMustBeInProgressRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle, hints: 2);

        AssertBrokenRule<GameMustBeInProgressRule>(() => game.UseHint());
    }

    [Test]
    public void UseHint_WhenNoHintsRemaining_BreaksHintAllowanceMustHaveRemainingRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle, hints: 1);
        game.Start();
        game.UseHint();

        AssertBrokenRule<HintAllowanceMustHaveRemainingRule>(() => game.UseHint());
    }
}
