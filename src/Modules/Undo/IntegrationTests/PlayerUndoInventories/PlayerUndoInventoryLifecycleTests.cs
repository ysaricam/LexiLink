using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.ConsumePlayerUndo;
using LexiLink.Modules.Undo.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Undo.IntegrationTests.PlayerUndoInventories;

[TestFixture]
public class PlayerUndoInventoryLifecycleTests : TestBase
{
    [Test]
    public async Task PlayerRegistered_OutboxProcessor_InitializesUndoInventoryAtDefaultSeed()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-undo-1", "Yasin", "en-US"));

        var beforeProcessing = await QuerySingleOrDefaultAsync<int?>("""
            SELECT 1
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        beforeProcessing.Should().BeNull(
            "Undo inventory should not exist until PlayerRegistered is dispatched");

        await ProcessOutboxAsync();

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(0);
    }

    [Test]
    public async Task PlayerRegistered_ReplayedForSamePlayer_DoesNotDuplicateUndoAggregate()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-undo-2", "Ada", "en-US"));
        await ProcessOutboxAsync();

        await EventsBus.PublishAsync(
            new PlayerRegisteredIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                playerId,
                "Ada",
                1234,
                "en-US",
                true));

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(1L);
    }

    [Test]
    public async Task ConsumePlayerUndo_DecrementsBalance()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-undo-3", "Mina", "en-US"));
        await ProcessOutboxAsync();

        await ExecuteSqlAsync("""
            UPDATE "undo"."PlayerUndoInventories"
            SET "Balance" = 2
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        await ExecuteCommandAsync(new ConsumePlayerUndoCommand(playerId, 1));

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(1);
    }

    [Test]
    public async Task ConsumePlayerUndo_WhenBalanceIsZero_Throws()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-undo-4", "Nora", "en-US"));
        await ProcessOutboxAsync();

        var act = async () => await ExecuteCommandAsync(new ConsumePlayerUndoCommand(playerId, 1));

        await act.Should().ThrowAsync<Exception>(
            "Consume on an empty undo inventory must surface the broken business rule");
    }
}
