using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.MakeStep;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Games.Undo;
using LexiLink.Modules.Games.Application.Games.UseHint;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

/// <summary>
/// Sprint UR regression: Undo has no per-game free quota anymore.
/// Every in-game undo must go through IUndoGuard before Game mutates.
/// </summary>
[TestFixture]
public class UseUndoFallThroughTests : TestBase
{
    [Test]
    public async Task Undo_CallsUndoGuard_WhenGuardAllows()
    {
        var setup = await StartGameAndMakeOneStepAsync();

        await ExecuteCommandAsync(new UndoCommand(setup.GameId));

        UndoGuard.CallCount.Should().Be(1,
            "every undo should be served from the player's external inventory via IUndoGuard");

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.History.Should().BeEmpty();
        details.UndosUsed.Should().Be(1);
    }

    [Test]
    public async Task Undo_PropagatesGuardException_AndDoesNotMutateGame()
    {
        var setup = await StartGameAndMakeOneStepAsync();
        UndoGuard.RejectNext = true;

        var act = async () => await ExecuteCommandAsync(new UndoCommand(setup.GameId));

        await act.Should().ThrowAsync<InvalidOperationException>();
        UndoGuard.CallCount.Should().Be(1);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.History.Should().HaveCount(1);
        details.UndosUsed.Should().Be(0,
            "guard rejection must happen before the Game undo counter increments");
    }

    private async Task<GameSetup> StartGameAndMakeOneStepAsync()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));
        var hint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));
        await ExecuteCommandAsync(new MakeStepCommand(setup.GameId, hint.RecommendedLinkId));
        HintGuard.Reset();

        return setup;
    }
}
