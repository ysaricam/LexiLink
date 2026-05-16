using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;
using LexiLink.Modules.Quests.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Quests.IntegrationTests.PlayerQuests;

[TestFixture]
public class QuestIntegrationEventTests : TestBase
{
    [Test]
    public async Task GameCompleted_IssuesAndProgresses_FirstThreeAndDailyQuests()
    {
        var playerId = Guid.NewGuid();

        await EventsBus.PublishAsync(GameCompletedFor(playerId));

        var rows = await QueryAsync<QuestRow>("""
            SELECT "QuestType", "State", "Progress", "Goal"
            FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId
            ORDER BY "QuestType";
        """, new { PlayerId = playerId });

        rows.Should().HaveCount(3);

        // FirstGameCompleted (goal=1) completes in one event.
        rows.Should().ContainEquivalentOf(new QuestRow(
            "FirstGameCompleted", "ReadyToClaim", 1, 1));

        // ThreeGamesCompleted (goal=3) advances to 1.
        rows.Should().ContainEquivalentOf(new QuestRow(
            "ThreeGamesCompleted", "Active", 1, 3));

        // DailyThreeGames (goal=3) advances to 1.
        rows.Should().ContainEquivalentOf(new QuestRow(
            "DailyThreeGames", "Active", 1, 3));
    }

    [Test]
    public async Task ThreeGameCompletedEvents_CompleteThreeGamesAndDailyQuests()
    {
        var playerId = Guid.NewGuid();

        for (var i = 0; i < 3; i++)
        {
            await EventsBus.PublishAsync(GameCompletedFor(playerId));
        }

        var rows = await QueryAsync<QuestRow>("""
            SELECT "QuestType", "State", "Progress", "Goal"
            FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId
            ORDER BY "QuestType";
        """, new { PlayerId = playerId });

        rows.Should().HaveCount(3);
        rows.Should().ContainEquivalentOf(new QuestRow(
            "FirstGameCompleted", "ReadyToClaim", 1, 1));
        rows.Should().ContainEquivalentOf(new QuestRow(
            "ThreeGamesCompleted", "ReadyToClaim", 3, 3));
        rows.Should().ContainEquivalentOf(new QuestRow(
            "DailyThreeGames", "ReadyToClaim", 3, 3));
    }

    [Test]
    public async Task AuthProviderLinked_DoesNotIssueAccountLinked_WhenThreeGamesNotClaimed()
    {
        var playerId = Guid.NewGuid();

        await EventsBus.PublishAsync(AuthProviderLinkedFor(playerId));

        var count = await QuerySingleOrDefaultAsync<long>("""
            SELECT COUNT(*) FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });

        count.Should().Be(0L,
            "AccountLinked must wait for ThreeGamesCompleted to be claimed before being issued");
    }

    [Test]
    public async Task AuthProviderLinked_IssuesAccountLinked_WhenThreeGamesClaimed()
    {
        var playerId = Guid.NewGuid();

        // Complete + claim ThreeGamesCompleted to satisfy AccountLinked's prerequisite.
        for (var i = 0; i < 3; i++)
        {
            await EventsBus.PublishAsync(GameCompletedFor(playerId));
        }

        var threeGamesQuestId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId AND "QuestType" = 'ThreeGamesCompleted';
        """, new { PlayerId = playerId });
        threeGamesQuestId.Should().NotBe(Guid.Empty);

        await QuestsModule.ExecuteCommandAsync(
            new ClaimQuestCommand(threeGamesQuestId, playerId));

        // Now AuthProviderLinked should issue AccountLinked and immediately complete it.
        await EventsBus.PublishAsync(AuthProviderLinkedFor(playerId));

        var accountLinked = await QuerySingleOrDefaultAsync<QuestRow>("""
            SELECT "QuestType", "State", "Progress", "Goal"
            FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId AND "QuestType" = 'AccountLinked';
        """, new { PlayerId = playerId });

        accountLinked.Should().NotBeNull();
        accountLinked!.State.Should().Be("ReadyToClaim");
        accountLinked.Progress.Should().Be(1);
        accountLinked.Goal.Should().Be(1);
    }

    [Test]
    public async Task ClaimQuest_QueuesAndProcessesQuestClaimedOutboxNotification()
    {
        var playerId = Guid.NewGuid();
        await EventsBus.PublishAsync(GameCompletedFor(playerId));

        var firstQuestId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId AND "QuestType" = 'FirstGameCompleted';
        """, new { PlayerId = playerId });
        firstQuestId.Should().NotBe(Guid.Empty);

        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(firstQuestId, playerId));

        var queued = await QuerySingleOrDefaultAsync<OutboxRow>("""
            SELECT "Type" AS "Type", "ProcessedDate" AS "ProcessedDate"
            FROM "quests"."OutboxMessages"
            ORDER BY "OccurredOn" DESC
            LIMIT 1;
        """);
        queued.Should().NotBeNull();
        queued!.Type.Should().Be("Quests.PlayerQuestClaimedDomainEventNotification");
        queued.ProcessedDate.Should().BeNull("outbox row should exist but not yet be processed");

        await ProcessOutboxAsync();

        var processed = await QuerySingleOrDefaultAsync<OutboxRow>("""
            SELECT "Type" AS "Type", "ProcessedDate" AS "ProcessedDate"
            FROM "quests"."OutboxMessages"
            ORDER BY "OccurredOn" DESC
            LIMIT 1;
        """);
        processed!.ProcessedDate.Should().NotBeNull("outbox processor should mark the row as processed");
    }

    private sealed class OutboxRow
    {
        public string Type { get; init; } = string.Empty;
        public DateTime? ProcessedDate { get; init; }
    }

    private static GameCompletedIntegrationEvent GameCompletedFor(Guid playerId) =>
        new(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            GameId: Guid.NewGuid(),
            PlayerId: playerId,
            StartLinkId: Guid.NewGuid(),
            TargetLinkId: Guid.NewGuid(),
            Score: 100);

    private static AuthProviderLinkedIntegrationEvent AuthProviderLinkedFor(Guid playerId) =>
        new(
            Id: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            PlayerId: playerId,
            Provider: "Apple",
            ExternalId: "external-id");

    private sealed record QuestRow(string QuestType, string State, int Progress, int Goal);
}
