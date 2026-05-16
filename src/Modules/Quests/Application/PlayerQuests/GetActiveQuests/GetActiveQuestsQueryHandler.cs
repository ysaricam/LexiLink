using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Quests.Application.Configuration.Queries;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;

internal class GetActiveQuestsQueryHandler : IQueryHandler<GetActiveQuestsQuery, IReadOnlyList<PlayerQuestDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetActiveQuestsQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public async Task<IReadOnlyList<PlayerQuestDto>> Handle(
        GetActiveQuestsQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Id"            AS "Id",
                "PlayerId"      AS "PlayerId",
                "QuestType"     AS "QuestType",
                "State"         AS "State",
                "Progress"      AS "Progress",
                "Goal"          AS "Goal",
                "RewardAmount"  AS "RewardAmount",
                "IssuedAt"      AS "IssuedAt",
                "CompletedAt"   AS "CompletedAt",
                "ClaimedAt"     AS "ClaimedAt",
                "ExpiresAt"     AS "ExpiresAt"
            FROM "quests"."v_PlayerQuests"
            WHERE "PlayerId" = @PlayerId
            ORDER BY "IssuedAt" DESC;
        """;

        var rows = await connection.QueryAsync<RawPlayerQuestRow>(
            new CommandDefinition(sql, new { query.PlayerId }, cancellationToken: cancellationToken));

        var now = _clock.UtcNow;
        return rows.Select(row => Project(row, now)).ToList();
    }

    private static PlayerQuestDto Project(RawPlayerQuestRow row, DateTime now)
    {
        var dbState = Enum.Parse<QuestState>(row.State);
        var effectiveState = ProjectState(dbState, row.ExpiresAt, now);

        return new PlayerQuestDto(
            Id: row.Id,
            PlayerId: row.PlayerId,
            QuestType: row.QuestType,
            State: effectiveState.ToString(),
            Progress: row.Progress,
            Goal: row.Goal,
            RewardAmount: row.RewardAmount,
            IssuedAt: DateTime.SpecifyKind(row.IssuedAt, DateTimeKind.Utc),
            CompletedAt: row.CompletedAt is null
                ? null
                : DateTime.SpecifyKind(row.CompletedAt.Value, DateTimeKind.Utc),
            ClaimedAt: row.ClaimedAt is null
                ? null
                : DateTime.SpecifyKind(row.ClaimedAt.Value, DateTimeKind.Utc),
            ExpiresAt: row.ExpiresAt is null
                ? null
                : DateTime.SpecifyKind(row.ExpiresAt.Value, DateTimeKind.Utc));
    }

    private static QuestState ProjectState(QuestState dbState, DateTime? expiresAt, DateTime now)
    {
        if (dbState != QuestState.Active && dbState != QuestState.ReadyToClaim)
        {
            return dbState;
        }

        if (expiresAt is null || now < expiresAt.Value)
        {
            return dbState;
        }

        return QuestState.Expired;
    }

    private sealed class RawPlayerQuestRow
    {
        public Guid Id { get; init; }
        public Guid PlayerId { get; init; }
        public string QuestType { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public int Progress { get; init; }
        public int Goal { get; init; }
        public int RewardAmount { get; init; }
        public DateTime IssuedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime? ClaimedAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
    }
}
