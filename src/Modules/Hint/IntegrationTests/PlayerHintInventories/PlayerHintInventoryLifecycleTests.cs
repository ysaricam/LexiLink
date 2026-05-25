using LexiLink.Modules.Hint.Application.PlayerHintInventories.ConsumePlayerHint;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.GrantHint;
using LexiLink.Modules.Hint.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Hint.IntegrationTests.PlayerHintInventories;

[TestFixture]
public class PlayerHintInventoryLifecycleTests : TestBase
{
    [Test]
    public async Task PlayerRegistered_OutboxProcessor_InitializesHintInventoryAtConfiguredSeed()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-hint-1", "Yasin", "en-US"));

        var beforeProcessing = await QuerySingleOrDefaultAsync<int?>("""
            SELECT 1
            FROM "hint"."PlayerHintInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        beforeProcessing.Should().BeNull(
            "Hint inventory should not exist until PlayerRegistered is dispatched");

        await ProcessOutboxAsync();

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "hint"."PlayerHintInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().NotBeNull();
        balance!.Value.Should().BeGreaterThanOrEqualTo(0,
            "default seed is 0 unless Hint:InitialBalance is configured otherwise");
    }

    [Test]
    public async Task PlayerRegistered_TwiceWithSameDeviceId_DoesNotDuplicateHintAggregate()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-hint-2", "Ada", "en-US"));
        await ProcessOutboxAsync();

        var samePlayerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-hint-2", "Ada", "en-US"));
        samePlayerId.Should().Be(playerId);
        await ProcessOutboxAsync();

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "hint"."PlayerHintInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(1L);
    }

    [Test]
    public async Task GrantHint_ThenConsume_DecrementsBalance()
    {
        var playerId = await ProvisionPlayerWithInventoryAsync("device-hint-3");

        await ExecuteCommandAsync(new GrantHintCommand(playerId, 3));
        var afterGrant = await ReadBalance(playerId);
        afterGrant.Should().Be(3);

        await ExecuteCommandAsync(new ConsumePlayerHintCommand(playerId, 1));
        var afterConsume = await ReadBalance(playerId);
        afterConsume.Should().Be(2);
    }

    [Test]
    public async Task Consume_WhenBalanceIsZero_Throws()
    {
        var playerId = await ProvisionPlayerWithInventoryAsync("device-hint-4");

        var act = async () => await ExecuteCommandAsync(new ConsumePlayerHintCommand(playerId, 1));

        await act.Should().ThrowAsync<Exception>(
            "Consume on an empty inventory must surface the broken business rule");
    }

    private async Task<Guid> ProvisionPlayerWithInventoryAsync(string deviceId)
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand(deviceId, "Test", "en-US"));
        await ProcessOutboxAsync();
        return playerId;
    }

    private async Task<int> ReadBalance(Guid playerId) =>
        (await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance" FROM "hint"."PlayerHintInventories" WHERE "PlayerId" = @PlayerId
        """, new { PlayerId = playerId })) ?? -1;
}
