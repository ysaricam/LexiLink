using LexiLink.Common.Application.Data;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using Dapper;

namespace LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;

internal class GetLinkOutgoingLinksQueryHandler : IQueryHandler<GetLinkOutgoingLinksQuery, List<OutgoingLinkDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetLinkOutgoingLinksQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<OutgoingLinkDto>> Handle(GetLinkOutgoingLinksQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Target"."Id" AS "Id",
                "Target"."Value" AS "Value",
                "Target"."IsActive" AS "IsActive"
            FROM "games"."LinkOutgoingLinks" AS "Outgoing"
            INNER JOIN "games"."v_Links" AS "Target"
                ON "Outgoing"."OutgoingLinkId" = "Target"."Id"
            WHERE "Outgoing"."LinkId" = @LinkId
        """;

        var results = await connection.QueryAsync<OutgoingLinkDto>(
            new CommandDefinition(
                sql,
                new { query.LinkId },
                cancellationToken: cancellationToken
            )
        );

        return results.AsList();
    }
}
