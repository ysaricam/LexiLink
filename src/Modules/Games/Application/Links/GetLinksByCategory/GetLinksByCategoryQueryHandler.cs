using LexiLink.Common.Application.Data;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using Dapper;

namespace LexiLink.Modules.Games.Application.Links.GetLinksByCategory;

internal class GetLinksByCategoryQueryHandler : IQueryHandler<GetLinksByCategoryQuery, List<LinkListItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetLinksByCategoryQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<LinkListItemDto>> Handle(GetLinksByCategoryQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                [Link].[Id] AS [Id],
                [Link].[Value] AS [Value],
                [Link].[IsActive] AS [IsActive]
            FROM [Games].[v_Links] AS [Link]
            WHERE [Link].[CategoryId] = @CategoryId
        """;

        var results = await connection.QueryAsync<LinkListItemDto>(
            new CommandDefinition(
                sql,
                new { query.CategoryId },
                cancellationToken: cancellationToken
            )
        );

        return results.AsList();
    }
}
