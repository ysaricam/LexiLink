using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Reset.Application.Configuration.Queries;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories;

namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.GetPlayerReset;

internal class GetPlayerResetQueryHandler : IQueryHandler<GetPlayerResetQuery, PlayerResetSnapshotDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerResetQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PlayerResetSnapshotDto> Handle(GetPlayerResetQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "PlayerId" AS "PlayerId",
                "Balance"  AS "Balance"
            FROM "reset"."PlayerResetInventories"
            WHERE "PlayerId" = @PlayerId;
        """;

        var snapshot = await connection.QuerySingleOrDefaultAsync<PlayerResetSnapshotDto>(
            new CommandDefinition(
                sql,
                new { query.PlayerId },
                cancellationToken: cancellationToken));

        if (snapshot is null)
        {
            throw new NotFoundException(nameof(PlayerResetInventory), query.PlayerId);
        }

        return snapshot;
    }
}
