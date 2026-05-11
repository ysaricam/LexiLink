using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Events;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameCreateTests : GameTestsBase
{
    [Test]
    public void Create_WithValidValues_RaisesGameCreatedDomainEvent()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);

        var game = Game.Create(NewPlayerId(), puzzle, maxSteps: 5, hints: 3, undos: 5, resets: 2);

        AssertPublishedDomainEvent<GameCreatedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }

    [Test]
    public void Create_StartsWithStateInitial()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);

        var game = BuildGame(puzzle);

        game.State.Should().Be(GameState.Initial);
    }

    [Test]
    public void Create_CurrentLinkIsStartLink()
    {
        var start = NewLinkId();
        var target = NewLinkId();
        var puzzle = BuildPuzzle(start, target, [target]);

        var game = BuildGame(puzzle);

        game.CurrentLinkId.Should().Be(start);
    }

    [Test]
    public void Create_HistoryIsEmpty()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);

        var game = BuildGame(puzzle);

        game.History.Should().BeEmpty();
    }

    [Test]
    public void Create_ScoreIsNull()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);

        var game = BuildGame(puzzle);

        game.Score.Should().BeNull();
    }
}
