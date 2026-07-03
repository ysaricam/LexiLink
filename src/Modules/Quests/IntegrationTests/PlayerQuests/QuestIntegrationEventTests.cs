using System.Text.Json;
using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;
using LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;
using LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Quests.IntegrationTests.PlayerQuests;

/// <summary>
/// Post Sprint Q1 there are no Game/Auth integration event handlers in
/// Quests — issuance is lazy, driven by <c>GET /quests/me</c>. These
/// tests verify the sync pass + claim outbox flow against the seeded
/// daily quest and admin-created definitions.
/// </summary>
[TestFixture]
public class QuestIntegrationEventTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");

    [Test]
    public async Task GetActiveQuests_FreshPlayer_LazilyIssuesSeedDailyQuest()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 0;

        var quests = await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        quests.Should().HaveCount(1);
        quests[0].QuestDefinitionId.Should().Be(SeedDailyQuestDefinitionId);
        quests[0].DisplayState.Should().Be(nameof(QuestState.Active));
        quests[0].Progress.Should().Be(0);
        quests[0].Threshold.Should().Be(3);

        var dbCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        dbCount.Should().Be(1, "lazy sync persisted the daily quest row");
    }

    [Test]
    public async Task GetActiveQuests_TwiceForSamePlayer_IsIdempotent()
    {
        var playerId = Guid.NewGuid();

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));
        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        var dbCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        dbCount.Should().Be(1, "ON CONFLICT DO NOTHING keeps the row count stable");
    }

    [Test]
    public async Task GetActiveQuests_ProjectsReadyToClaim_WhenDailyCounterMeetsThresholdBeforeFirstSync()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 5; // above threshold (3)

        var quests = await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        quests.Should().HaveCount(1);
        quests[0].Progress.Should().Be(3);
        quests[0].DisplayState.Should().Be("ReadyToClaim");
    }

    [Test]
    public async Task GetActiveQuests_PrereqUnclaimed_DoesNotIssueDownstream()
    {
        AdminContext.LoginAs(AdminId);
        var prereqId = await QuestsModule.ExecuteCommandAsync(new CreateQuestDefinitionCommand(
            name: "Bronz",
            description: "1 oyun",
            trigger: QuestTrigger.GameCompletedTotal,
            threshold: 1,
            energyReward: 3,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot));
        var downstreamId = await QuestsModule.ExecuteCommandAsync(new CreateQuestDefinitionCommand(
            name: "Gümüş",
            description: "3 oyun",
            trigger: QuestTrigger.GameCompletedTotal,
            threshold: 3,
            energyReward: 5,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
            prerequisiteQuestDefinitionId: prereqId,
            progressBaseline: ProgressBaseline.FromSnapshot));
        AdminContext.Logout();

        var playerId = Guid.NewGuid();

        var quests = await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        quests.Should().NotContain(q => q.QuestDefinitionId == downstreamId);
        quests.Should().Contain(q => q.QuestDefinitionId == prereqId);
    }

    [Test]
    public async Task GetActiveQuests_PrereqClaimed_IssuesDownstream()
    {
        AdminContext.LoginAs(AdminId);
        var prereqId = await QuestsModule.ExecuteCommandAsync(new CreateQuestDefinitionCommand(
            name: "Bronz",
            description: "1 oyun",
            trigger: QuestTrigger.GameCompletedTotal,
            threshold: 1,
            energyReward: 3,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot));
        var downstreamId = await QuestsModule.ExecuteCommandAsync(new CreateQuestDefinitionCommand(
            name: "Gümüş",
            description: "3 oyun",
            trigger: QuestTrigger.GameCompletedTotal,
            threshold: 3,
            energyReward: 5,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
            prerequisiteQuestDefinitionId: prereqId,
            progressBaseline: ProgressBaseline.FromSnapshot));
        AdminContext.Logout();

        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedTotal = 0;

        // First sync: prereq issued, downstream not yet.
        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        // Player completes a game and claims the prereq.
        QuestCounterReader.GamesCompletedTotal = 1;
        var prereqRowId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId AND "QuestDefinitionId" = @QuestDefinitionId;
        """, new { PlayerId = playerId, QuestDefinitionId = prereqId });
        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(prereqRowId, playerId));

        // Second sync: downstream should now appear.
        var quests = await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        quests.Should().Contain(q => q.QuestDefinitionId == downstreamId);
    }

    [Test]
    public async Task GetActiveQuests_DeletesExpiredDailyRows_OnSync()
    {
        var playerId = Guid.NewGuid();

        // First sync: daily quest issued with expiry = next UTC midnight.
        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        // Force the existing daily row to look expired.
        var expiredAt = DateTime.UtcNow.AddDays(-1);
        await using (var connection = new Npgsql.NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            await Dapper.SqlMapper.ExecuteAsync(connection, """
                UPDATE "quests"."PlayerQuests"
                SET "ExpiresAt" = @ExpiresAt
                WHERE "PlayerId" = @PlayerId;
            """, new { PlayerId = playerId, ExpiresAt = expiredAt });
        }

        // Next sync: expired Active row is deleted, then re-issued because
        // the daily definition is still active.
        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        var rows = await QueryAsync<DateTime?>("""
            SELECT "ExpiresAt" FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId;
        """, new { PlayerId = playerId });
        rows.Should().HaveCount(1);
        rows[0].Should().NotBeNull();
        rows[0]!.Value.Should().BeAfter(DateTime.UtcNow, "sync re-issued the daily quest with a fresh expiry");
    }

    [Test]
    public async Task GetActiveQuests_DeletesExpiredClaimedDailyRows_OnSync()
    {
        var playerId = Guid.NewGuid();

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        var expiredAt = DateTime.UtcNow.AddDays(-1);
        await using (var connection = new Npgsql.NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            await Dapper.SqlMapper.ExecuteAsync(connection, """
                UPDATE "quests"."PlayerQuests"
                SET
                    "State" = 'Claimed',
                    "ClaimedAt" = @ClaimedAt,
                    "ExpiresAt" = @ExpiresAt
                WHERE "PlayerId" = @PlayerId;
            """, new { PlayerId = playerId, ClaimedAt = expiredAt.AddHours(-1), ExpiresAt = expiredAt });
        }

        var quests = await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        quests.Should().ContainSingle(q => q.QuestDefinitionId == SeedDailyQuestDefinitionId);
        var daily = quests.Single(q => q.QuestDefinitionId == SeedDailyQuestDefinitionId);
        daily.DisplayState.Should().Be(nameof(QuestState.Active));
        daily.Progress.Should().Be(0);
        daily.ExpiresAt.Should().NotBeNull();
        daily.ExpiresAt!.Value.Should().BeAfter(DateTime.UtcNow);

        var rows = await QueryAsync<(string State, DateTime? ClaimedAt, DateTime? ExpiresAt)>("""
            SELECT "State", "ClaimedAt", "ExpiresAt"
            FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId
              AND "QuestDefinitionId" = @QuestDefinitionId;
        """, new { PlayerId = playerId, QuestDefinitionId = SeedDailyQuestDefinitionId });
        rows.Should().ContainSingle();
        rows[0].State.Should().Be(nameof(QuestState.Active));
        rows[0].ClaimedAt.Should().BeNull();
        rows[0].ExpiresAt.Should().NotBeNull();
        rows[0].ExpiresAt!.Value.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public async Task ClaimQuest_AtThreshold_QueuesQuestClaimedOutboxNotification()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 0;

        AdminContext.LoginAs(AdminId);
        await QuestsModule.ExecuteCommandAsync(new UpdateQuestDefinitionCommand(
            questDefinitionId: SeedDailyQuestDefinitionId,
            description: "Bugün 3 oyun tamamla.",
            threshold: 3,
            energyReward: 5,
            hintReward: 2,
            undoReward: 1,
            resetReward: 1,
            diamondReward: 3,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot));
        AdminContext.Logout();

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        // Player now completes 3 games today, hitting the threshold.
        QuestCounterReader.GamesCompletedToday = 3;

        var questId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId LIMIT 1;
        """, new { PlayerId = playerId });
        questId.Should().NotBe(Guid.Empty);

        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        var queued = await QuerySingleOrDefaultAsync<OutboxRow>("""
            SELECT "Type" AS "Type", "Data" AS "Data", "ProcessedDate" AS "ProcessedDate"
            FROM "quests"."OutboxMessages"
            WHERE "Type" = 'Quests.PlayerQuestClaimedDomainEventNotification'
            ORDER BY "OccurredOn" DESC
            LIMIT 1;
        """);
        queued.Should().NotBeNull();
        queued!.Type.Should().Be("Quests.PlayerQuestClaimedDomainEventNotification");
        queued.ProcessedDate.Should().BeNull("outbox row should exist but not yet be processed");
        using (var json = JsonDocument.Parse(queued.Data))
        {
            var root = json.RootElement;
            root.GetProperty("EnergyReward").GetInt32().Should().Be(0,
                "quest energy is now granted synchronously by the claim command");
            root.GetProperty("HintReward").GetInt32().Should().Be(2);
            root.GetProperty("UndoReward").GetInt32().Should().Be(1);
            root.GetProperty("ResetReward").GetInt32().Should().Be(1);
            root.GetProperty("DiamondReward").GetInt32().Should().Be(3);
        }

        await ProcessOutboxAsync();

        var processed = await QuerySingleOrDefaultAsync<OutboxRow>("""
            SELECT "Type" AS "Type", "Data" AS "Data", "ProcessedDate" AS "ProcessedDate"
            FROM "quests"."OutboxMessages"
            WHERE "Type" = 'Quests.PlayerQuestClaimedDomainEventNotification'
            ORDER BY "OccurredOn" DESC
            LIMIT 1;
        """);
        processed!.ProcessedDate.Should().NotBeNull("outbox processor should mark the row as processed");
    }

    [Test]
    public async Task ClaimQuest_WhenEnergyRewardPartiallyFits_LeavesRemainingEnergyClaimable()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 0;

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));
        QuestCounterReader.GamesCompletedToday = 3;

        var questId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId LIMIT 1;
        """, new { PlayerId = playerId });

        QuestEnergyRewardGrant.GrantedAmount = 4;
        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        var partial = await QuerySingleOrDefaultAsync<PlayerQuestRewardState>("""
            SELECT "State" AS "State", "RemainingEnergyReward" AS "RemainingEnergyReward"
            FROM "quests"."PlayerQuests"
            WHERE "Id" = @QuestId;
        """, new { QuestId = questId });

        partial.Should().NotBeNull();
        partial!.State.Should().Be(nameof(QuestState.Active));
        partial.RemainingEnergyReward.Should().Be(1);
        QuestEnergyRewardGrant.LastRequestedAmount.Should().Be(5);

        var quests = await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));
        var dto = quests.Single(q => q.Id == questId);
        dto.DisplayState.Should().Be("ReadyToClaim");
        dto.EnergyReward.Should().Be(1);

        QuestEnergyRewardGrant.GrantedAmount = 1;
        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        var completed = await QuerySingleOrDefaultAsync<PlayerQuestRewardState>("""
            SELECT "State" AS "State", "RemainingEnergyReward" AS "RemainingEnergyReward"
            FROM "quests"."PlayerQuests"
            WHERE "Id" = @QuestId;
        """, new { QuestId = questId });

        completed.Should().NotBeNull();
        completed!.State.Should().Be(nameof(QuestState.Claimed));
        completed.RemainingEnergyReward.Should().Be(0);
        QuestEnergyRewardGrant.LastRequestedAmount.Should().Be(1);
    }

    [Test]
    public async Task ClaimQuest_WhenEnergyOnlyRewardCannotFit_BreaksBusinessRuleAndKeepsQuestActive()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 0;

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));
        QuestCounterReader.GamesCompletedToday = 3;

        var questId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId LIMIT 1;
        """, new { PlayerId = playerId });

        QuestEnergyRewardGrant.GrantedAmount = 0;

        var act = async () => await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        await act.Should().ThrowAsync<Common.Domain.BusinessRuleValidationException>();

        var state = await QuerySingleOrDefaultAsync<PlayerQuestRewardState>("""
            SELECT "State" AS "State", "RemainingEnergyReward" AS "RemainingEnergyReward"
            FROM "quests"."PlayerQuests"
            WHERE "Id" = @QuestId;
        """, new { QuestId = questId });

        state.Should().NotBeNull();
        state!.State.Should().Be(nameof(QuestState.Active));
        state.RemainingEnergyReward.Should().Be(5);
    }

    [Test]
    public async Task ClaimQuest_WhenEnergyIsPartial_DoesNotQueueNonEnergyRewardsTwice()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 0;

        AdminContext.LoginAs(AdminId);
        await QuestsModule.ExecuteCommandAsync(new UpdateQuestDefinitionCommand(
            questDefinitionId: SeedDailyQuestDefinitionId,
            description: "Bugün 3 oyun tamamla.",
            threshold: 3,
            energyReward: 5,
            hintReward: 2,
            undoReward: 1,
            resetReward: 1,
            diamondReward: 3,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot));
        AdminContext.Logout();

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));
        QuestCounterReader.GamesCompletedToday = 3;

        var questId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId LIMIT 1;
        """, new { PlayerId = playerId });

        QuestEnergyRewardGrant.GrantedAmount = 4;
        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        var queuedAfterFirstClaim = await QueryAsync<OutboxRow>("""
            SELECT "Type" AS "Type", "Data" AS "Data", "ProcessedDate" AS "ProcessedDate"
            FROM "quests"."OutboxMessages"
            WHERE "Type" = 'Quests.PlayerQuestClaimedDomainEventNotification';
        """);
        queuedAfterFirstClaim.Should().HaveCount(1);
        using (var json = JsonDocument.Parse(queuedAfterFirstClaim[0].Data))
        {
            var root = json.RootElement;
            root.GetProperty("EnergyReward").GetInt32().Should().Be(0);
            root.GetProperty("HintReward").GetInt32().Should().Be(2);
            root.GetProperty("UndoReward").GetInt32().Should().Be(1);
            root.GetProperty("ResetReward").GetInt32().Should().Be(1);
            root.GetProperty("DiamondReward").GetInt32().Should().Be(3);
        }

        QuestEnergyRewardGrant.GrantedAmount = 1;
        await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        var queuedAfterSecondClaim = await QueryAsync<OutboxRow>("""
            SELECT "Type" AS "Type", "Data" AS "Data", "ProcessedDate" AS "ProcessedDate"
            FROM "quests"."OutboxMessages"
            WHERE "Type" = 'Quests.PlayerQuestClaimedDomainEventNotification';
        """);
        queuedAfterSecondClaim.Should().HaveCount(1,
            "non-energy rewards must not be granted again while collecting leftover energy");
    }

    [Test]
    public async Task ClaimQuest_BelowThreshold_BreaksBusinessRule()
    {
        var playerId = Guid.NewGuid();
        QuestCounterReader.GamesCompletedToday = 0;

        await QuestsModule.ExecuteQueryAsync(new GetActiveQuestsQuery(playerId));

        var questId = await QuerySingleOrDefaultAsync<Guid>("""
            SELECT "Id" FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId LIMIT 1;
        """, new { PlayerId = playerId });

        // Threshold not reached.
        QuestCounterReader.GamesCompletedToday = 1;

        var act = async () => await QuestsModule.ExecuteCommandAsync(new ClaimQuestCommand(questId, playerId));

        await act.Should().ThrowAsync<Common.Domain.BusinessRuleValidationException>();
    }

    private sealed class OutboxRow
    {
        public string Type { get; init; } = string.Empty;
        public string Data { get; init; } = string.Empty;
        public DateTime? ProcessedDate { get; init; }
    }

    private sealed class PlayerQuestRewardState
    {
        public string State { get; init; } = string.Empty;
        public int RemainingEnergyReward { get; init; }
    }
}
