using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Games.UseHint;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

/// <summary>
/// Sprint H regression: validate that the first UseHint per Game
/// consumes the per-game free quota (1) and does NOT call the
/// IHintGuard sync gateway, but subsequent UseHints fall through to
/// the gateway. The gateway either allows (decrements the player's
/// hint inventory in the real Hint module) or throws when the
/// inventory is empty.
/// </summary>
[TestFixture]
public class UseHintFallThroughTests : TestBase
{
    [Test]
    public async Task FirstHint_ConsumesFreeQuota_DoesNotCallHintGuard()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var hint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        hint.Should().NotBeNull();
        HintGuard.CallCount.Should().Be(0,
            "the free per-game hint must satisfy the request without invoking the gateway");

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.HintsUsed.Should().Be(1);
    }

    [Test]
    public async Task SecondHint_FallsThroughToHintGuard_WhenGuardAllows()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var firstHint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));
        var secondHint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        firstHint.Should().NotBeNull();
        secondHint.Should().NotBeNull();
        HintGuard.CallCount.Should().Be(1,
            "the second hint should be served from the player's external inventory via IHintGuard");

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        // HintsUsed only tracks the per-game free quota by design — the
        // external inventory path does not increment it. After the first
        // free hint the counter stays at 1 even though a second hint
        // was served from the inventory.
        details.HintsUsed.Should().Be(1);
    }

    [Test]
    public async Task SecondHint_PropagatesGuardException_WhenInventoryIsEmpty()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        await ExecuteCommandAsync(new UseHintCommand(setup.GameId));
        HintGuard.RejectNext = true;

        var act = async () => await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        await act.Should().ThrowAsync<InvalidOperationException>();
        HintGuard.CallCount.Should().Be(1);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.HintsUsed.Should().Be(1,
            "free-quota counter is unaffected by gateway rejection");
    }
}
