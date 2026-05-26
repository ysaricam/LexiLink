using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.ConsumePlayerReset;
using LexiLink.Modules.Reset.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Reset.IntegrationTests.PlayerResetInventories;

[TestFixture]
public class PlayerResetInventoryLifecycleTests : TestBase
{
    [Test]
    public async Task PlayerRegistered_OutboxProcessor_InitializesResetInventoryAtDefaultSeed()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-reset-1", "Yasin", "en-US"));

        var beforeProcessing = await QuerySingleOrDefaultAsync<int?>("""
            SELECT 1
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        beforeProcessing.Should().BeNull(
            "Reset inventory should not exist until PlayerRegistered is dispatched");

        await ProcessOutboxAsync();

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(0);
    }

    [Test]
    public async Task PlayerRegistered_ReplayedForSamePlayer_DoesNotDuplicateResetAggregate()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-reset-2", "Ada", "en-US"));
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
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(1L);
    }

    [Test]
    public async Task ConsumePlayerReset_DecrementsBalance()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-reset-3", "Mina", "en-US"));
        await ProcessOutboxAsync();

        await ExecuteSqlAsync("""
            UPDATE "reset"."PlayerResetInventories"
            SET "Balance" = 2
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        await ExecuteCommandAsync(new ConsumePlayerResetCommand(playerId, 1));

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(1);
    }

    [Test]
    public async Task ConsumePlayerReset_WhenBalanceIsZero_Throws()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-reset-4", "Nora", "en-US"));
        await ProcessOutboxAsync();

        var act = async () => await ExecuteCommandAsync(new ConsumePlayerResetCommand(playerId, 1));

        await act.Should().ThrowAsync<Exception>(
            "Consume on an empty reset inventory must surface the broken business rule");
    }
}
