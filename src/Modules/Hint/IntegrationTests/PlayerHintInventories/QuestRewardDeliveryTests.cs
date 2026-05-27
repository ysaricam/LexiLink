using LexiLink.Modules.Hint.IntegrationTests.SeedWork;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Quests.IntegrationEvents;

namespace LexiLink.Modules.Hint.IntegrationTests.PlayerHintInventories;

[TestFixture]
public class QuestRewardDeliveryTests : TestBase
{
    [Test]
    public async Task QuestClaimed_GrantsHintReward()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-hint-quest-1", "Yasin", "en-US"));
        await ProcessOutboxAsync();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 0,
            HintReward: 2,
            UndoReward: 0,
            ResetReward: 0,
            DiamondReward: 0));

        var balance = await ReadBalanceAsync(playerId);
        balance.Should().Be(2);
    }

    [Test]
    public async Task QuestClaimed_WithZeroHintReward_DoesNotInitializeInventory()
    {
        var playerId = Guid.NewGuid();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 5,
            HintReward: 0,
            UndoReward: 1,
            ResetReward: 1,
            DiamondReward: 0));

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "hint"."PlayerHintInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(0L);
    }

    private async Task<int?> ReadBalanceAsync(Guid playerId) =>
        await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "hint"."PlayerHintInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
}
