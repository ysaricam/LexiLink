using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Games;

internal class CompletedGameLinkPairRepository : ICompletedGameLinkPairRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal CompletedGameLinkPairRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyCollection<CompletedGameLinkPair>> GetCompletedPairsAsync(
        Guid playerId,
        CategoryId categoryId,
        CancellationToken cancellationToken = default)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Game"."StartLinkId"  AS "StartLinkId",
                "Game"."TargetLinkId" AS "TargetLinkId"
            FROM "games"."Games" AS "Game"
            WHERE "Game"."PlayerId" = @PlayerId
              AND "Game"."CategoryId" = @CategoryId
              AND "Game"."State" = 'Completed';
        """;

        var rows = await connection.QueryAsync<CompletedGameLinkPairRow>(
            new CommandDefinition(
                sql,
                new { PlayerId = playerId, CategoryId = categoryId.Value },
                cancellationToken: cancellationToken));

        return rows
            .Select(row => new CompletedGameLinkPair(new LinkId(row.StartLinkId), new LinkId(row.TargetLinkId)))
            .ToList();
    }

    private record CompletedGameLinkPairRow(Guid StartLinkId, Guid TargetLinkId);
}
