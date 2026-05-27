using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Games.UseHint;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

/// <summary>
/// Post-UR1 model: Games has no per-game free hint allowance. Every
/// UseHint falls through to the player's persistent inventory via
/// IHintGuard. The gateway either allows (decrements the player's
/// hint inventory in the real Hint module) or throws when inventory is
/// empty.
/// </summary>
[TestFixture]
public class UseHintFallThroughTests : TestBase
{
    [Test]
    public async Task FirstHint_FallsThroughToHintGuard_WhenGuardAllows()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var hint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        hint.Should().NotBeNull();
        HintGuard.CallCount.Should().Be(1,
            "every hint should be served from the player's external inventory via IHintGuard");

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.HintsUsed.Should().Be(0,
            "there is no per-game free hint counter to consume");
    }

    [Test]
    public async Task MultipleHints_FallThroughToHintGuard_WhenGuardAllows()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var firstHint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));
        var secondHint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        firstHint.Should().NotBeNull();
        secondHint.Should().NotBeNull();
        HintGuard.CallCount.Should().Be(2,
            "each hint should be served from the player's external inventory via IHintGuard");

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.HintsUsed.Should().Be(0,
            "external inventory usage does not increment the removed free-hint counter");
    }

    [Test]
    public async Task FirstHint_PropagatesGuardException_WhenInventoryIsEmpty()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        HintGuard.RejectNext = true;

        var act = async () => await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        await act.Should().ThrowAsync<InvalidOperationException>();
        HintGuard.CallCount.Should().Be(1);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.HintsUsed.Should().Be(0,
            "there is no per-game free hint counter and failed gateway calls do not mutate the game");
    }
}
