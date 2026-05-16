using LexiLink.Modules.Games.Application.Games.AbandonGame;
using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.MakeStep;
using LexiLink.Modules.Games.Application.Games.Reset;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Games.Undo;
using LexiLink.Modules.Games.Application.Games.UseHint;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

[TestFixture]
public class GameIntegrationTests : TestBase
{
    [Test]
    public async Task CreateGame_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.Should().NotBeNull();
        details.PlayerId.Should().Be(setup.PlayerId);
        details.CategoryId.Should().Be(setup.CategoryId);
        details.State.Should().Be(GameState.Initial);
        details.Score.Should().BeNull();
        details.History.Should().BeEmpty();
    }

    [Test]
    public async Task StartGame_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);

        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.State.Should().Be(GameState.InProgress);
    }

    [Test]
    public async Task MakeStep_AndCompleteGame_Test()
    {
        // Use a long enough chain so any random start has a path of depth ≥3.
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var started = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        var orderedWords = new[] { "cat", "mat", "bat", "bag", "bug", "rug" };
        var orderedLinkIds = orderedWords.Select(word => setup.LinksByValue[word]).ToArray();
        var startIndex = Array.IndexOf(orderedLinkIds, started.StartLinkId);
        var targetIndex = Array.IndexOf(orderedLinkIds, started.TargetLinkId);
        var direction = startIndex < targetIndex ? 1 : -1;

        for (var index = startIndex + direction; index != targetIndex + direction; index += direction)
        {
            await ExecuteCommandAsync(new MakeStepCommand(setup.GameId, orderedLinkIds[index]));
        }

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.State.Should().Be(GameState.Completed);
        details.Score.Should().NotBeNull();
        details.History.Should().NotBeEmpty();
        details.CurrentLinkId.Should().Be(details.TargetLinkId);
    }

    [Test]
    public async Task UseHint_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var hint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));

        hint.Should().NotBeNull();
        hint.RecommendedLinkId.Should().NotBe(Guid.Empty);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.HintsUsed.Should().Be(1);
    }

    [Test]
    public async Task Undo_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));
        var hint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));
        await ExecuteCommandAsync(new MakeStepCommand(setup.GameId, hint.RecommendedLinkId));

        await ExecuteCommandAsync(new UndoCommand(setup.GameId));

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.History.Should().BeEmpty();
        details.UndosUsed.Should().Be(1);
    }

    [Test]
    public async Task Reset_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));
        var hint = await ExecuteCommandAsync(new UseHintCommand(setup.GameId));
        await ExecuteCommandAsync(new MakeStepCommand(setup.GameId, hint.RecommendedLinkId));

        await ExecuteCommandAsync(new ResetCommand(setup.GameId));

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.History.Should().BeEmpty();
        details.ResetsUsed.Should().Be(1);
        details.CurrentLinkId.Should().Be(details.StartLinkId);
    }

    [Test]
    public async Task AbandonGame_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        await ExecuteCommandAsync(new AbandonGameCommand(setup.GameId));

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));
        details.State.Should().Be(GameState.Abandoned);
    }

    [Test]
    public async Task GetGameById_ReturnsDenormalizedWords_Test()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);

        var details = await ExecuteQueryAsync(new GetGameByIdQuery(setup.GameId));

        details.StartWord.Should().NotBeNullOrEmpty();
        details.TargetWord.Should().NotBeNullOrEmpty();
        details.CurrentWord.Should().Be(details.StartWord);
    }
}
