using LexiLink.Common.Application.Data;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using Dapper;

namespace LexiLink.Modules.Games.Application.Categories.GetCategories;

internal class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryListItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetCategoriesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<CategoryListItemDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Category"."Id" AS "Id",
                "Category"."Name" AS "Name",
                "Category"."Language" AS "Language"
            FROM "games"."v_Categories" AS "Category"
            WHERE (@Locale IS NULL OR "Category"."Language" = @Locale)
            ORDER BY "Category"."Name"
        """;

        var results = await connection.QueryAsync<CategoryListItemDto>(
            new CommandDefinition(
                sql,
                new { query.Locale },
                cancellationToken: cancellationToken
            )
        );

        return results.AsList();
    }
}
