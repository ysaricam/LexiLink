using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.MakeStep;
using LexiLink.Modules.Games.Application.Games.Reset;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Games.UseHint;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

/// <summary>
/// Sprint UR regression: Reset has no per-game free quota anymore.
/// Every in-game reset must go through IResetGuard before Game mutates.
/// </summary>
[TestFixture]
public class ResetFallThroughTests : TestBase
{
    [Test]
    public async Task Reset_CallsResetGuard_WhenGuardAllows()
    {
        var setup = await StartGameAndMakeOneStepAsync();

        await ExecuteCommandAsync(new ResetCommand(setup.GameId));

        ResetGuard.CallCount.Should().Be(1,
            "every reset should be served from the player's external inventory via IResetGuard");

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.History.Should().BeEmpty();
        details.CurrentLinkId.Should().Be(details.StartLinkId);
        details.ResetsUsed.Should().Be(1);
    }

    [Test]
    public async Task Reset_PropagatesGuardException_AndDoesNotMutateGame()
    {
        var setup = await StartGameAndMakeOneStepAsync();
        var before = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        ResetGuard.RejectNext = true;

        var act = async () => await ExecuteCommandAsync(new ResetCommand(setup.GameId));

        await act.Should().ThrowAsync<InvalidOperationException>();
        ResetGuard.CallCount.Should().Be(1);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.History.Should().HaveCount(1);
        details.CurrentLinkId.Should().Be(before.CurrentLinkId);
        details.ResetsUsed.Should().Be(0,
            "guard rejection must happen before the Game reset counter increments");
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
