using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Players.Application.Configuration.Queries;

namespace LexiLink.Modules.Players.Application.Players.GetPlayerById;

internal class GetPlayerByIdQueryHandler : IQueryHandler<GetPlayerByIdQuery, PlayerDetailsDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PlayerDetailsDto> Handle(GetPlayerByIdQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Player"."Id"                 AS "Id",
                "Player"."DisplayName"        AS "DisplayName",
                "Player"."DiscriminatorValue" AS "Discriminator",
                "Player"."Handle"             AS "Handle",
                "Player"."AvatarUrl"          AS "AvatarUrl",
                "Player"."Locale"             AS "Locale",
                "Player"."IsGuest"            AS "IsGuest"
            FROM "players"."v_Players" AS "Player"
            WHERE "Player"."Id" = @PlayerId;

            SELECT
                "Auth"."Provider"   AS "Provider",
                "Auth"."ExternalId" AS "ExternalId",
                "Auth"."Email"      AS "Email",
                "Auth"."LinkedAt"   AS "LinkedAt"
            FROM "players"."PlayerAuthIdentities" AS "Auth"
            WHERE "Auth"."PlayerId" = @PlayerId
            ORDER BY "Auth"."LinkedAt" ASC;
        """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { query.PlayerId },
                cancellationToken: cancellationToken
            )
        );

        var dto = await multi.ReadSingleOrDefaultAsync<PlayerDetailsDto>()
            ?? throw new NotFoundException(nameof(PlayerDetailsDto), query.PlayerId);

        var authIdentities = (await multi.ReadAsync<AuthIdentityDto>()).ToList();

        return dto with { AuthIdentities = authIdentities };
    }
}
