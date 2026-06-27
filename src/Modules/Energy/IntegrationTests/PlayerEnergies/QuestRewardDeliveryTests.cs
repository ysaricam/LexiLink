using LexiLink.Modules.Energy.IntegrationTests.SeedWork;
using LexiLink.Modules.Energy.Application.PlayerEnergies.ConsumePlayerEnergy;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Quests.IntegrationEvents;

namespace LexiLink.Modules.Energy.IntegrationTests.PlayerEnergies;

[TestFixture]
public class QuestRewardDeliveryTests : TestBase
{
    [Test]
    public async Task QuestClaimed_GrantsEnergyOnlyUpToMaximum()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-quest-1", "Yasin", "en-US"));
        await ProcessOutboxAsync();

        var before = await QuerySingleOrDefaultAsync<int>("""
            SELECT "CurrentAmount" FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        before.Should().BeGreaterThan(0);

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 3,
            HintReward: 0,
            UndoReward: 0,
            ResetReward: 0,
            DiamondReward: 0));

        var after = await QuerySingleOrDefaultAsync<int>("""
            SELECT "CurrentAmount" FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        after.Should().Be(before,
            "player starts at maximum, so quest energy reward must not push current above max");
    }

    [Test]
    public async Task QuestClaimed_WhenPartiallyEmpty_CapsRewardAtMaximum()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-quest-partial", "Yasin", "en-US"));
        await ProcessOutboxAsync();

        await ExecuteCommandAsync(new ConsumePlayerEnergyCommand(playerId, 2));

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 5,
            HintReward: 0,
            UndoReward: 0,
            ResetReward: 0,
            DiamondReward: 0));

        var snapshot = await QuerySingleOrDefaultAsync<EnergySnapshot>("""
            SELECT "CurrentAmount" AS "CurrentAmount", "MaximumAmount" AS "MaximumAmount"
            FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        snapshot.Should().NotBeNull();
        snapshot!.CurrentAmount.Should().Be(snapshot.MaximumAmount,
            "3/5 plus a 5-energy reward should grant only the missing 2 energy");
    }

    [Test]
    public async Task QuestClaimed_BeforePlayerRegistered_LazilyInitializesEnergyThenGrants()
    {
        // Skip the PlayerRegistered flow — simulate the race where QuestClaimed
        // lands first. The Energy QuestClaimed handler runs EnsurePlayerEnergyExists
        // first, so the aggregate is created and then bonus is granted.
        var playerId = Guid.NewGuid();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 5,
            HintReward: 0,
            UndoReward: 0,
            ResetReward: 0,
            DiamondReward: 0));

        var snapshot = await QuerySingleOrDefaultAsync<EnergySnapshot>("""
            SELECT "CurrentAmount" AS "CurrentAmount", "MaximumAmount" AS "MaximumAmount"
            FROM "energy"."PlayerEnergies" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        snapshot.Should().NotBeNull();
        snapshot!.CurrentAmount.Should().Be(snapshot.MaximumAmount,
            "energy should be initialized full and the quest reward must not push above max");
    }

    private sealed class EnergySnapshot
    {
        public int CurrentAmount { get; init; }
        public int MaximumAmount { get; init; }
    }
}
