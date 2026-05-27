using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ConsumePlayerDiamond;
using LexiLink.Modules.Diamond.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.IntegrationEvents;

namespace LexiLink.Modules.Diamond.IntegrationTests.PlayerDiamondInventories;

[TestFixture]
public class PlayerDiamondInventoryLifecycleTests : TestBase
{
    [Test]
    public async Task PlayerRegistered_OutboxProcessor_InitializesDiamondInventoryAtDefaultSeed()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-diamond-1", "Yasin", "en-US"));

        var beforeProcessing = await QuerySingleOrDefaultAsync<int?>("""
            SELECT 1
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        beforeProcessing.Should().BeNull(
            "Diamond inventory should not exist until PlayerRegistered is dispatched");

        await ProcessOutboxAsync();

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(0);
    }

    [Test]
    public async Task PlayerRegistered_ReplayedForSamePlayer_DoesNotDuplicateDiamondAggregate()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-diamond-2", "Ada", "en-US"));
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
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(1L);
    }

    [Test]
    public async Task ConsumePlayerDiamond_DecrementsBalance()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-diamond-3", "Mina", "en-US"));
        await ProcessOutboxAsync();

        await ExecuteSqlAsync("""
            UPDATE "diamond"."PlayerDiamondInventories"
            SET "Balance" = 2
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        await ExecuteCommandAsync(new ConsumePlayerDiamondCommand(playerId, 1));

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(1);
    }

    [Test]
    public async Task ConsumePlayerDiamond_WhenBalanceIsZero_Throws()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-diamond-4", "Nora", "en-US"));
        await ProcessOutboxAsync();

        var act = async () => await ExecuteCommandAsync(new ConsumePlayerDiamondCommand(playerId, 1));

        await act.Should().ThrowAsync<Exception>(
            "Consume on an empty Diamond inventory must surface the broken business rule");
    }
}
