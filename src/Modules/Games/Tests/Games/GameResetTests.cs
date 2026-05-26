using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameResetTests : GameTestsBase
{
    [Test]
    public void ResetToStart_AfterSomeSteps_ClearsHistoryAndReturnsToStart_RaisesResetUsedDomainEvent()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [n1, target]);
        var game = BuildGame(puzzle, maxSteps: 5);
        game.Start();
        game.MakeStep(n1, LinearNeighborResolver(start, n1, target), FixedScoreCalculator());
        DomainEventsTestHelper.ClearAllDomainEvents(game);

        game.ResetToStart();

        game.History.Should().BeEmpty();
        game.CurrentLinkId.Should().Be(start);
        game.State.Should().Be(GameState.InProgress);
        AssertPublishedDomainEvent<ResetUsedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }

    [Test]
    public void ResetToStart_WhenHistoryIsEmpty_BreaksGameHistoryMustNotBeEmptyRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);
        game.Start();

        AssertBrokenRule<GameHistoryMustNotBeEmptyRule>(game.ResetToStart);
    }

    [Test]
    public void ResetToStart_WhenNotStarted_BreaksGameMustBeInProgressRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);

        AssertBrokenRule<GameMustBeInProgressRule>(game.ResetToStart);
    }

    [Test]
    public void ResetToStart_CanBeUsedRepeatedly_WhenHistoryExists()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, maxSteps: 5);
        game.Start();
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());
        game.ResetToStart();
        DomainEventsTestHelper.ClearAllDomainEvents(game);
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());

        game.ResetToStart();

        game.History.Should().BeEmpty();
        game.CurrentLinkId.Should().Be(start);
        AssertPublishedDomainEvent<ResetUsedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }
}
