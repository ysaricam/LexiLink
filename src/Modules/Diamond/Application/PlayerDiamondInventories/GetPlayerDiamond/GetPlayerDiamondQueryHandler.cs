using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Diamond.Application.Configuration.Queries;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GetPlayerDiamond;

internal class GetPlayerDiamondQueryHandler : IQueryHandler<GetPlayerDiamondQuery, PlayerDiamondSnapshotDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerDiamondQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PlayerDiamondSnapshotDto> Handle(
        GetPlayerDiamondQuery query,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "PlayerId" AS "PlayerId",
                "Balance"  AS "Balance"
            FROM "diamond"."PlayerDiamondInventories"
            WHERE "PlayerId" = @PlayerId;
        """;

        var snapshot = await connection.QuerySingleOrDefaultAsync<PlayerDiamondSnapshotDto>(
            new CommandDefinition(
                sql,
                new { query.PlayerId },
                cancellationToken: cancellationToken));

        if (snapshot is null)
        {
            throw new NotFoundException(nameof(PlayerDiamondInventory), query.PlayerId);
        }

        return snapshot;
    }
}
