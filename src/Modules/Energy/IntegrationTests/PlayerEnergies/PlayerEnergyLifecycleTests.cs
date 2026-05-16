using LexiLink.Modules.Energy.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

namespace LexiLink.Modules.Energy.IntegrationTests.PlayerEnergies;

[TestFixture]
public class PlayerEnergyLifecycleTests : TestBase
{
    [Test]
    public async Task PlayerRegistered_OutboxProcessor_InitializesPlayerEnergyAtMaximum()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-energy-1", "Yasin", "en-US"));

        var beforeProcessing = await QuerySingleOrDefaultAsync<int?>("""
            SELECT 1
            FROM "energy"."PlayerEnergies"
            WHERE "PlayerId" = @PlayerId;
        """,
            new { PlayerId = playerId });
        beforeProcessing.Should().BeNull("Energy aggregate should not exist until the integration event is dispatched");

        await ProcessOutboxAsync();

        var snapshot = await QuerySingleOrDefaultAsync<EnergyRow>("""
            SELECT
                "CurrentAmount"           AS "CurrentAmount",
                "MaximumAmount"           AS "MaximumAmount",
                "RechargeIntervalSeconds" AS "RechargeIntervalSeconds"
            FROM "energy"."PlayerEnergies"
            WHERE "PlayerId" = @PlayerId;
        """,
            new { PlayerId = playerId });

        snapshot.Should().NotBeNull();
        snapshot!.CurrentAmount.Should().Be(snapshot.MaximumAmount, "newly initialized player energy starts full");
        snapshot.MaximumAmount.Should().BeGreaterThan(0);
        snapshot.RechargeIntervalSeconds.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task PlayerRegistered_TwiceWithSameDeviceId_DoesNotDuplicateEnergyAggregate()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-energy-2", "Ada", "en-US"));

        await ProcessOutboxAsync();

        var samePlayerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-energy-2", "Ada", "en-US"));
        samePlayerId.Should().Be(playerId, "idempotent guest registration should return the same player id");

        await ProcessOutboxAsync();

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "energy"."PlayerEnergies"
            WHERE "PlayerId" = @PlayerId;
        """,
            new { PlayerId = playerId });

        rowCount.Should().Be(1L);
    }

    private sealed class EnergyRow
    {
        public int CurrentAmount { get; init; }
        public int MaximumAmount { get; init; }
        public int RechargeIntervalSeconds { get; init; }
    }
}
