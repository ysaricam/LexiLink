using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Queries;
using Dapper;

namespace LexiLink.Modules.Games.Application.Links.GetLinkDetails;

internal class GetLinkDetailsQueryHandler : IQueryHandler<GetLinkDetailsQuery, LinkDetailsDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetLinkDetailsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<LinkDetailsDto> Handle(GetLinkDetailsQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                [Link].[Id] AS [Id],
                [Link].[CategoryId] AS [CategoryId],
                [Link].[Value] AS [Value],
                [Link].[Description] AS [Description],
                [Link].[IsActive] AS [IsActive]
            FROM [Games].[v_Links] AS [Link]
            WHERE [Link].[Id] = @LinkId
        """;

        var dto = await connection.QuerySingleOrDefaultAsync<LinkDetailsDto>(
            new CommandDefinition(
                sql,
                new { query.LinkId },
                cancellationToken: cancellationToken
            )
        );

        return dto ?? throw new NotFoundException(nameof(LinkDetailsDto), query.LinkId);
    }
}