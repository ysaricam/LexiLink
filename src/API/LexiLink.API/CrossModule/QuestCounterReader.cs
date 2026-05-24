using Dapper;
using LexiLink.Modules.Quests.Application.Configuration.CrossModule;
using Npgsql;

namespace LexiLink.API.CrossModule;

/// <summary>
/// API-host adapter for the Quests → Stats / Players sync gateway. Reads
/// the three player counters Quests needs to issue, progress and claim
/// quests: <c>GamesCompleted</c> (total + daily) from Stats and
/// <c>AuthProviderLinked</c> from Players. Bypasses the module facades
/// because there is no command-shaped abstraction that fits a hot-path
/// counter read; module isolation is preserved by keeping the SQL inside
/// the composition root (no Quests reference to Stats or Players).
/// </summary>
internal class QuestCounterReader : IQuestCounterReader
{
    private readonly string _connectionString;

    public QuestCounterReader(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<QuestCounters> ReadAsync(
        Guid playerId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string totalSql = """
            SELECT COALESCE("GamesCompleted", 0)
            FROM "stats"."PlayerStats"
            WHERE "PlayerId" = @PlayerId;
        """;
        var total = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(totalSql, new { PlayerId = playerId }, cancellationToken: cancellationToken));

        const string dailySql = """
            SELECT COALESCE("GamesCompleted", 0)
            FROM "stats"."PlayerPeriodStats"
            WHERE "PeriodType" = 'Daily'
              AND "PeriodStartDate" = @PeriodStartDate
              AND "PlayerId" = @PlayerId;
        """;
        var daily = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                dailySql,
                new { PlayerId = playerId, PeriodStartDate = nowUtc.Date },
                cancellationToken: cancellationToken));

        const string authLinkedSql = """
            SELECT EXISTS(
                SELECT 1
                FROM "players"."PlayerAuthIdentities"
                WHERE "PlayerId" = @PlayerId
            );
        """;
        var authLinked = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(authLinkedSql, new { PlayerId = playerId }, cancellationToken: cancellationToken));

        return new QuestCounters(
            GamesCompletedTotal: total ?? 0,
            GamesCompletedToday: daily ?? 0,
            AuthProviderLinked: authLinked);
    }
}
