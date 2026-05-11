using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using Dapper;

namespace LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;

internal class GetCategoryDetailsQueryHandler : IQueryHandler<GetCategoryDetailsQuery, CategoryDetailsDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetCategoryDetailsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<CategoryDetailsDto> Handle(GetCategoryDetailsQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Category"."Id" AS "Id",
                "Category"."Name" AS "Name",
                "Category"."Description" AS "Description",
                (
                    SELECT COUNT(*)::int
                    FROM "games"."v_Links" AS "Link"
                    WHERE "Link"."CategoryId" = "Category"."Id"
                ) AS "LinkCount"
            FROM "games"."v_Categories" AS "Category"
            WHERE "Category"."Id" = @CategoryId
        """;

        var dto = await connection.QuerySingleOrDefaultAsync<CategoryDetailsDto>(
            new CommandDefinition(
                sql,
                new { query.CategoryId },
                cancellationToken: cancellationToken
            )
        );

        return dto ?? throw new NotFoundException(nameof(CategoryDetailsDto), query.CategoryId);
    }
}
