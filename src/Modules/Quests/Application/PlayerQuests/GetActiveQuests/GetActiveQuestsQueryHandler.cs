using System.Data;
using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;
using LexiLink.Modules.Quests.Application.Configuration.Queries;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;

/// <summary>
/// Two-pass handler. The first pass mutates: it issues missing
/// PlayerQuest rows for every active QuestDefinition the player is now
/// eligible for and deletes expired daily rows. The second pass reads
/// the final set, joins it with Stats counters in memory, and computes
/// progress + display state for the API. Lazy issuance replaces the
/// pre-Sprint-Q1 eager broadcast on PlayerRegistered + game-completed
/// integration events.
/// </summary>
internal class GetActiveQuestsQueryHandler : IQueryHandler<GetActiveQuestsQuery, IReadOnlyList<PlayerQuestDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IQuestCounterReader _counterReader;
    private readonly IClock _clock;

    internal GetActiveQuestsQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IQuestCounterReader counterReader,
        IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _counterReader = counterReader;
        _clock = clock;
    }

    public async Task<IReadOnlyList<PlayerQuestDto>> Handle(
        GetActiveQuestsQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();
        var now = _clock.UtcNow;
        var counters = await _counterReader.ReadAsync(query.PlayerId, now, cancellationToken);

        // Delete first, then sync — an expired Daily row in the DB would
        // otherwise look "existing" to the sync pass and prevent re-issue
        // for the new day. Claimed daily rows must expire too; otherwise a
        // yesterday claim blocks today's daily quest through the unique
        // PlayerId + QuestDefinitionId slot.
        await DeleteExpiredDailyPlayerQuestsAsync(connection, query.PlayerId, now, cancellationToken);
        await SyncMissingPlayerQuestsAsync(connection, query.PlayerId, counters, now, cancellationToken);

        var rows = await ReadPlayerQuestsAsync(connection, query.PlayerId, cancellationToken);
        return rows.Select(r => Project(r, counters, now)).ToList();
    }

    private static async Task SyncMissingPlayerQuestsAsync(
        IDbConnection connection,
        Guid playerId,
        QuestCounters counters,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string definitionsSql = """
            SELECT
                "Id",
                "Name",
                "Description",
                "Trigger",
                "Threshold",
                "EnergyReward",
                "HintReward",
                "UndoReward",
                "ResetReward",
                "DiamondReward",
                "PrerequisiteQuestDefinitionId",
                "ProgressBaseline",
                "IsActive"
            FROM "quests"."QuestDefinitions"
            WHERE "IsActive" = TRUE;
        """;
        var definitions = (await connection.QueryAsync<RawQuestDefinitionRow>(
            new CommandDefinition(definitionsSql, cancellationToken: cancellationToken))).ToList();

        if (definitions.Count == 0)
        {
            return;
        }

        const string existingSql = """
            SELECT "QuestDefinitionId", "State"
            FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId;
        """;
        var existing = (await connection.QueryAsync<(Guid QuestDefinitionId, string State)>(
            new CommandDefinition(existingSql, new { PlayerId = playerId }, cancellationToken: cancellationToken)))
            .ToList();

        var anyExisting = existing.Select(e => e.QuestDefinitionId).ToHashSet();
        var claimed = existing
            .Where(e => string.Equals(e.State, nameof(QuestState.Claimed), StringComparison.Ordinal))
            .Select(e => e.QuestDefinitionId)
            .ToHashSet();

        const string insertSql = """
            INSERT INTO "quests"."PlayerQuests"
                ("Id", "PlayerId", "QuestDefinitionId", "ProgressBaselineSnapshot",
                 "RemainingEnergyReward", "NonEnergyRewardsClaimed",
                 "State", "IssuedAt", "ClaimedAt", "ExpiresAt")
            VALUES
                (@Id, @PlayerId, @QuestDefinitionId, @ProgressBaselineSnapshot,
                 @RemainingEnergyReward, FALSE,
                 @State, @IssuedAt, NULL, @ExpiresAt)
            ON CONFLICT ("PlayerId", "QuestDefinitionId") DO NOTHING;
        """;

        foreach (var def in definitions)
        {
            if (anyExisting.Contains(def.Id))
            {
                continue;
            }

            if (def.PrerequisiteQuestDefinitionId is { } prereq && !claimed.Contains(prereq))
            {
                continue;
            }

            var trigger = Enum.Parse<QuestTrigger>(def.Trigger);
            var baseline = Enum.Parse<ProgressBaseline>(def.ProgressBaseline);
            var baselineSnapshot = ComputeBaselineSnapshot(trigger, baseline, counters);
            var expiresAt = trigger == QuestTrigger.GameCompletedDaily
                ? (DateTime?)NextUtcMidnight(now)
                : null;

            await connection.ExecuteAsync(new CommandDefinition(
                insertSql,
                new
                {
                    Id = Guid.NewGuid(),
                    PlayerId = playerId,
                    QuestDefinitionId = def.Id,
                    ProgressBaselineSnapshot = baselineSnapshot,
                    RemainingEnergyReward = def.EnergyReward,
                    State = nameof(QuestState.Active),
                    IssuedAt = now,
                    ExpiresAt = expiresAt,
                },
                cancellationToken: cancellationToken));
        }
    }

    private static Task DeleteExpiredDailyPlayerQuestsAsync(
        IDbConnection connection,
        Guid playerId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM "quests"."PlayerQuests"
            WHERE "PlayerId" = @PlayerId
              AND "ExpiresAt" IS NOT NULL
              AND "ExpiresAt" <= @Now;
        """;
        return connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { PlayerId = playerId, Now = now },
            cancellationToken: cancellationToken));
    }

    private static async Task<IReadOnlyList<RawPlayerQuestRow>> ReadPlayerQuestsAsync(
        IDbConnection connection,
        Guid playerId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pq."Id"                       AS "Id",
                pq."PlayerId"                 AS "PlayerId",
                pq."QuestDefinitionId"        AS "QuestDefinitionId",
                pq."ProgressBaselineSnapshot" AS "ProgressBaselineSnapshot",
                pq."RemainingEnergyReward"    AS "RemainingEnergyReward",
                pq."State"                    AS "State",
                pq."IssuedAt"                 AS "IssuedAt",
                pq."ClaimedAt"                AS "ClaimedAt",
                pq."ExpiresAt"                AS "ExpiresAt",
                qd."Name"                     AS "Name",
                qd."Description"              AS "Description",
                qd."Trigger"                  AS "Trigger",
                qd."Threshold"                AS "Threshold",
                qd."EnergyReward"             AS "EnergyReward",
                qd."HintReward"               AS "HintReward",
                qd."UndoReward"               AS "UndoReward",
                qd."ResetReward"              AS "ResetReward",
                qd."DiamondReward"            AS "DiamondReward"
            FROM "quests"."PlayerQuests" AS pq
            INNER JOIN "quests"."QuestDefinitions" AS qd
                ON qd."Id" = pq."QuestDefinitionId"
            WHERE pq."PlayerId" = @PlayerId
              AND qd."IsActive" = TRUE
            ORDER BY pq."IssuedAt" DESC;
        """;
        var rows = await connection.QueryAsync<RawPlayerQuestRow>(
            new CommandDefinition(sql, new { PlayerId = playerId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    private static PlayerQuestDto Project(RawPlayerQuestRow row, QuestCounters counters, DateTime now)
    {
        var trigger = Enum.Parse<QuestTrigger>(row.Trigger);
        var currentCounter = CurrentCounterFor(trigger, counters);
        var rawProgress = currentCounter - row.ProgressBaselineSnapshot;
        var progress = Math.Clamp(rawProgress, 0, row.Threshold);

        var dbState = Enum.Parse<QuestState>(row.State);
        var displayState = ComputeDisplayState(dbState, progress, row.Threshold);

        return new PlayerQuestDto(
            Id: row.Id,
            PlayerId: row.PlayerId,
            QuestDefinitionId: row.QuestDefinitionId,
            Name: row.Name,
            Description: row.Description,
            Trigger: row.Trigger,
            DisplayState: displayState,
            Progress: progress,
            Threshold: row.Threshold,
            EnergyReward: dbState == QuestState.Claimed ? 0 : row.RemainingEnergyReward,
            HintReward: row.HintReward,
            UndoReward: row.UndoReward,
            ResetReward: row.ResetReward,
            DiamondReward: row.DiamondReward,
            IssuedAt: DateTime.SpecifyKind(row.IssuedAt, DateTimeKind.Utc),
            ClaimedAt: row.ClaimedAt is null
                ? null
                : DateTime.SpecifyKind(row.ClaimedAt.Value, DateTimeKind.Utc),
            ExpiresAt: row.ExpiresAt is null
                ? null
                : DateTime.SpecifyKind(row.ExpiresAt.Value, DateTimeKind.Utc));
    }

    private static int CurrentCounterFor(QuestTrigger trigger, QuestCounters counters) =>
        trigger switch
        {
            QuestTrigger.GameCompletedDaily => counters.GamesCompletedToday,
            QuestTrigger.GameCompletedTotal => counters.GamesCompletedTotal,
            QuestTrigger.AuthProviderLinked => counters.AuthProviderLinked ? 1 : 0,
            _                               => 0,
        };

    private static int ComputeBaselineSnapshot(
        QuestTrigger trigger,
        ProgressBaseline baseline,
        QuestCounters counters) =>
        trigger switch
        {
            QuestTrigger.GameCompletedDaily   => 0,
            QuestTrigger.AuthProviderLinked   => 0,
            QuestTrigger.GameCompletedTotal   =>
                baseline == ProgressBaseline.FromExistingTotal ? 0 : counters.GamesCompletedTotal,
            _                                 => 0,
        };

    private static string ComputeDisplayState(QuestState dbState, int progress, int threshold)
    {
        if (dbState == QuestState.Claimed)
        {
            return nameof(QuestState.Claimed);
        }

        return progress >= threshold ? "ReadyToClaim" : nameof(QuestState.Active);
    }

    private static DateTime NextUtcMidnight(DateTime now)
    {
        var utcNow = now.Kind == DateTimeKind.Utc ? now : now.ToUniversalTime();
        return DateTime.SpecifyKind(utcNow.Date.AddDays(1), DateTimeKind.Unspecified);
    }

    private sealed class RawQuestDefinitionRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Trigger { get; init; } = string.Empty;
        public int Threshold { get; init; }
        public int EnergyReward { get; init; }
        public int HintReward { get; init; }
        public int UndoReward { get; init; }
        public int ResetReward { get; init; }
        public int DiamondReward { get; init; }
        public Guid? PrerequisiteQuestDefinitionId { get; init; }
        public string ProgressBaseline { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }

    private sealed class RawPlayerQuestRow
    {
        public Guid Id { get; init; }
        public Guid PlayerId { get; init; }
        public Guid QuestDefinitionId { get; init; }
        public int ProgressBaselineSnapshot { get; init; }
        public int RemainingEnergyReward { get; init; }
        public string State { get; init; } = string.Empty;
        public DateTime IssuedAt { get; init; }
        public DateTime? ClaimedAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Trigger { get; init; } = string.Empty;
        public int Threshold { get; init; }
        public int EnergyReward { get; init; }
        public int HintReward { get; init; }
        public int UndoReward { get; init; }
        public int ResetReward { get; init; }
        public int DiamondReward { get; init; }
    }
}
