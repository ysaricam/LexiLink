using LexiLink.Modules.Diamond.IntegrationTests.SeedWork;
using LexiLink.Modules.Quests.IntegrationEvents;

namespace LexiLink.Modules.Diamond.IntegrationTests.PlayerDiamondInventories;

[TestFixture]
public class QuestRewardDeliveryTests : TestBase
{
    [Test]
    public async Task QuestClaimed_GrantsDiamondReward()
    {
        var playerId = Guid.NewGuid();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 0,
            HintReward: 0,
            UndoReward: 0,
            ResetReward: 0,
            DiamondReward: 2));

        var balance = await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        balance.Should().Be(2);
    }

    [Test]
    public async Task QuestClaimed_WithZeroDiamondReward_DoesNotInitializeInventory()
    {
        var playerId = Guid.NewGuid();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 5,
            HintReward: 1,
            UndoReward: 1,
            ResetReward: 1,
            DiamondReward: 0));

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(0L);
    }
}
