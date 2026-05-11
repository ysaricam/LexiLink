using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameUndoTests : GameTestsBase
{
    [Test]
    public void Undo_AfterStep_RemovesLastHistoryEntryAndRaisesUndoUsedDomainEvent()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [n1, target]);
        var game = BuildGame(puzzle, maxSteps: 5, undos: 2);
        game.Start();
        game.MakeStep(n1, LinearNeighborResolver(start, n1, target), FixedScoreCalculator());
        DomainEventsTestHelper.ClearAllDomainEvents(game);

        game.Undo();

        game.History.Should().BeEmpty();
        game.CurrentLinkId.Should().Be(start);
        AssertPublishedDomainEvent<UndoUsedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }

    [Test]
    public void Undo_WhenHistoryIsEmpty_BreaksGameHistoryMustNotBeEmptyRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle, undos: 2);
        game.Start();

        AssertBrokenRule<GameHistoryMustNotBeEmptyRule>(game.Undo);
    }

    [Test]
    public void Undo_WhenNotStarted_BreaksGameMustBeInProgressRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle, undos: 2);

        AssertBrokenRule<GameMustBeInProgressRule>(game.Undo);
    }

    [Test]
    public void Undo_FromLastStepWarning_TransitionsBackToInProgress()
    {
        // maxSteps=3, push two steps to enter LastStepWarning, then Undo.
        var start = NewLinkId();
        var n1 = NewLinkId();
        var n2 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, maxSteps: 3, undos: 5);
        game.Start();
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());
        game.MakeStep(n2, NeighborResolver((n1, [n2])), FixedScoreCalculator());
        game.State.Should().Be(GameState.LastStepWarning);

        game.Undo();

        game.State.Should().Be(GameState.InProgress);
    }

    [Test]
    public void Undo_WhenNoUndosRemaining_BreaksUndoAllowanceMustHaveRemainingRule()
    {
        var start = NewLinkId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);
        var game = BuildGame(puzzle, maxSteps: 5, undos: 1);
        game.Start();
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());
        game.Undo();
        game.MakeStep(n1, NeighborResolver((start, [n1])), FixedScoreCalculator());

        AssertBrokenRule<UndoAllowanceMustHaveRemainingRule>(game.Undo);
    }
}
