using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Hint.Application.Configuration.Queries;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.GetPlayerHint;

internal class GetPlayerHintQueryHandler : IQueryHandler<GetPlayerHintQuery, PlayerHintSnapshotDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerHintQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PlayerHintSnapshotDto> Handle(GetPlayerHintQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "PlayerId" AS "PlayerId",
                "Balance"  AS "Balance"
            FROM "hint"."PlayerHintInventories"
            WHERE "PlayerId" = @PlayerId;
        """;

        var snapshot = await connection.QuerySingleOrDefaultAsync<PlayerHintSnapshotDto>(
            new CommandDefinition(
                sql,
                new { query.PlayerId },
                cancellationToken: cancellationToken));

        if (snapshot is null)
        {
            throw new NotFoundException(nameof(PlayerHintInventory), query.PlayerId);
        }

        return snapshot;
    }
}
