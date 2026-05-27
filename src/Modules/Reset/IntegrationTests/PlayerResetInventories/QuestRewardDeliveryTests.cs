using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Quests.IntegrationEvents;
using LexiLink.Modules.Reset.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Reset.IntegrationTests.PlayerResetInventories;

[TestFixture]
public class QuestRewardDeliveryTests : TestBase
{
    [Test]
    public async Task QuestClaimed_GrantsResetReward()
    {
        var playerId = await ExecuteCommandAsync(
            new RegisterGuestPlayerCommand("device-reset-quest-1", "Yasin", "en-US"));
        await ProcessOutboxAsync();

        await EventsBus.PublishAsync(new QuestClaimedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            PlayerQuestId: Guid.NewGuid(),
            QuestDefinitionId: Guid.NewGuid(),
            EnergyReward: 0,
            HintReward: 0,
            UndoReward: 0,
            ResetReward: 2,
            DiamondReward: 0));

        var balance = await ReadBalanceAsync(playerId);
        balance.Should().Be(2);
    }

    [Test]
    public async Task QuestClaimed_WithZeroResetReward_DoesNotInitializeInventory()
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
            ResetReward: 0,
            DiamondReward: 0));

        var rowCount = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*)
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        rowCount.Should().Be(0L);
    }

    private async Task<int?> ReadBalanceAsync(Guid playerId) =>
        await QuerySingleOrDefaultAsync<int?>("""
            SELECT "Balance"
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
}
