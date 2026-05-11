using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Rules;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class GameStartTests : GameTestsBase
{
    [Test]
    public void Start_FromInitial_TransitionsToInProgressAndRaisesGameStartedDomainEvent()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);

        game.Start();

        game.State.Should().Be(GameState.InProgress);
        AssertPublishedDomainEvent<GameStartedDomainEvent>(game)
            .GameId.Should().Be(game.Id);
    }

    [Test]
    public void Start_WhenAlreadyStarted_BreaksGameMustBeNotStartedRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);
        game.Start();

        AssertBrokenRule<GameMustBeNotStartedRule>(game.Start);
    }

    [Test]
    public void Start_WhenAbandoned_BreaksGameMustBeNotStartedRule()
    {
        var puzzle = BuildPuzzle(NewLinkId(), NewLinkId(), [NewLinkId()]);
        var game = BuildGame(puzzle);
        game.Abandon();

        AssertBrokenRule<GameMustBeNotStartedRule>(game.Start);
    }
}
