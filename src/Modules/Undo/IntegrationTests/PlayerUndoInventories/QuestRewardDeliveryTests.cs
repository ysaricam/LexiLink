using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Quests.IntegrationEvents;
using LexiLink.Modules.Undo.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Undo.IntegrationTests.PlayerUndoInventories;

[TestFixture]
public class QuestRewardDeliveryTests : TestBase
{
    [Test]
    public async Task QuestClaimed_GrantsUndoReward()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-undo-quest-1", "Yasin", "en-US"));
        await ProcessOutboxAsync();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 0,
            HintReward: 0,
            UndoReward: 2,
            ResetReward: 0));

        var balance = await ReadBalanceAsync(playerId);
        balance.Should().Be(2);
    }

    [Test]
    public async Task QuestClaimed_WithZeroUndoReward_DoesNotInitializeInventory()
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
            UndoReward: 0,
            ResetReward: 1));

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(0L);
    }

    private async Task<int?> ReadBalanceAsync(Guid playerId) =>
        await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "undo"."PlayerUndoInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
}
