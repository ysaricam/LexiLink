using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Rules;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameAbandonTests : GameTestsBase
{
    [Test]
    public void Abandon_FromInitial_TransitionsToAbandonedAndRaisesGameAbandonedDomainEvent()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);

        game.Abandon();

        game.State.Should().Be(GameState.Abandoned);
        AssertPublishedDomainEvent<GameAbandonedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }

    [Test]
    public void Abandon_FromInProgress_TransitionsToAbandoned()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);
        game.Start();

        game.Abandon();

        game.State.Should().Be(GameState.Abandoned);
    }

    [Test]
    public void Abandon_WhenAlreadyAbandoned_BreaksGameMustNotBeFinishedRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);
        game.Abandon();

        AssertBrokenRule<GameMustNotBeFinishedRule>(game.Abandon);
    }

    [Test]
    public void Abandon_WhenAlreadyCompleted_BreaksGameMustNotBeFinishedRule()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, maxSteps: 5);
        game.Start();
        game.MakeStep(target, LinearNeighborResolver(start, target), FixedScoreCalculator());

        AssertBrokenRule<GameMustNotBeFinishedRule>(game.Abandon);
    }

    [Test]
    public void Abandon_WhenAlreadyFailed_BreaksGameMustNotBeFinishedRule()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, maxSteps: 1);
        game.Start();
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());

        AssertBrokenRule<GameMustNotBeFinishedRule>(game.Abandon);
    }
}
