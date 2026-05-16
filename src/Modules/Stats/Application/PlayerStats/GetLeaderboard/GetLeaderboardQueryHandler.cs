using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Stats.Application.Configuration.Queries;

namespace LexiLink.Modules.Stats.Application.PlayerStats.GetLeaderboard;

internal class GetLeaderboardQueryHandler :
    IQueryHandler<GetLeaderboardQuery, IReadOnlyList<LeaderboardEntryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal GetLeaderboardQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> Handle(
        GetLeaderboardQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();
        return query.Period == LeaderboardPeriod.AllTime
            ? await GetAllTimeLeaderboardAsync(connection, query)
            : await GetPeriodLeaderboardAsync(connection, query);
    }

    private static async Task<IReadOnlyList<LeaderboardEntryDto>> GetAllTimeLeaderboardAsync(
        System.Data.IDbConnection connection,
        GetLeaderboardQuery query)
    {
        var orderColumn = GetAllTimeOrderColumn(query.OrderBy);

        var entries = await connection.QueryAsync<LeaderboardEntryDto>(
            $"""
            SELECT
                "PlayerId",
                "DisplayName",
                "Discriminator",
                "Handle",
                "AvatarUrl",
                "Locale",
                "GamesCompleted",
                "BestScore",
                "TotalScore",
                "LastGameCompletedOn"
            FROM "stats"."v_PlayerStats"
            WHERE "GamesCompleted" > 0
            ORDER BY {orderColumn} DESC NULLS LAST,
                     "LastGameCompletedOn" ASC NULLS LAST,
                     "PlayerId" ASC
            LIMIT @Limit
            """,
            new { query.Limit });

        return entries.AsList();
    }

    private async Task<IReadOnlyList<LeaderboardEntryDto>> GetPeriodLeaderboardAsync(
        System.Data.IDbConnection connection,
        GetLeaderboardQuery query)
    {
        var periodStartDate = query.PeriodStartDate ?? GetCurrentPeriodStartDate(query.Period);
        var periodType = query.Period switch
        {
            LeaderboardPeriod.Daily => "Daily",
            LeaderboardPeriod.Weekly => "Weekly",
            _ => throw new ApplicationException($"Leaderboard period '{query.Period}' is not supported.")
        };
        var orderColumn = GetPeriodOrderColumn(query.OrderBy);

        var entries = await connection.QueryAsync<LeaderboardEntryDto>(
            $"""
            SELECT
                period."PlayerId",
                player_stats."DisplayName",
                player_stats."Discriminator",
                player_stats."Handle",
                player_stats."AvatarUrl",
                player_stats."Locale",
                period."GamesCompleted",
                period."BestScore",
                period."TotalScore",
                period."LastGameCompletedOn"
            FROM "stats"."PlayerPeriodStats" period
            LEFT JOIN "stats"."v_PlayerStats" player_stats
                ON player_stats."PlayerId" = period."PlayerId"
            WHERE period."PeriodType" = @PeriodType
              AND period."PeriodStartDate" = @PeriodStartDate
              AND period."GamesCompleted" > 0
            ORDER BY {orderColumn} DESC NULLS LAST,
                     period."LastGameCompletedOn" ASC NULLS LAST,
                     period."PlayerId" ASC
            LIMIT @Limit
            """,
            new
            {
                query.Limit,
                PeriodType = periodType,
                PeriodStartDate = periodStartDate
            });

        return entries.AsList();
    }

    private DateTime GetCurrentPeriodStartDate(LeaderboardPeriod period)
    {
        var today = _clock.UtcNow.Date;
        return period switch
        {
            LeaderboardPeriod.Daily => today,
            LeaderboardPeriod.Weekly => GetWeekStartDate(today),
            _ => today
        };
    }

    private static DateTime GetWeekStartDate(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    private static string GetAllTimeOrderColumn(LeaderboardOrderBy orderBy) =>
        orderBy switch
        {
            LeaderboardOrderBy.BestScore => "\"BestScore\"",
            LeaderboardOrderBy.TotalScore => "\"TotalScore\"",
            LeaderboardOrderBy.GamesCompleted => "\"GamesCompleted\"",
            _ => "\"BestScore\""
        };

    private static string GetPeriodOrderColumn(LeaderboardOrderBy orderBy) =>
        orderBy switch
        {
            LeaderboardOrderBy.BestScore => "period.\"BestScore\"",
            LeaderboardOrderBy.TotalScore => "period.\"TotalScore\"",
            LeaderboardOrderBy.GamesCompleted => "period.\"GamesCompleted\"",
            _ => "period.\"BestScore\""
        };
}
