using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Stats.Application.Configuration.Queries;

namespace LexiLink.Modules.Stats.Application.PlayerStats.GetPlayerStats;

internal class GetPlayerStatsQueryHandler : IQueryHandler<GetPlayerStatsQuery, PlayerStatsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerStatsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PlayerStatsDto?> Handle(
        GetPlayerStatsQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        return await connection.QuerySingleOrDefaultAsync<PlayerStatsDto>(
            """
            SELECT
                "PlayerId",
                "DisplayName",
                "Discriminator",
                "Handle",
                "AvatarUrl",
                "Locale",
                "IsGuest",
                "AuthProvidersLinked",
                "GamesCompleted",
                "BestScore",
                "TotalScore",
                "LastGameCompletedOn",
                "CreatedAt",
                "UpdatedAt"
            FROM "stats"."v_PlayerStats"
            WHERE "PlayerId" = @PlayerId
            """,
            new { query.PlayerId });
    }
}
