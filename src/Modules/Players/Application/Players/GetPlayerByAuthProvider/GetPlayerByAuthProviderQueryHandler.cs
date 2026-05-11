using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Players.Application.Configuration.Queries;
using LexiLink.Modules.Players.Application.Players.GetPlayerById;

namespace LexiLink.Modules.Players.Application.Players.GetPlayerByAuthProvider;

internal class GetPlayerByAuthProviderQueryHandler : IQueryHandler<GetPlayerByAuthProviderQuery, PlayerDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPlayerByAuthProviderQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PlayerDetailsDto?> Handle(GetPlayerByAuthProviderQuery query, CancellationToken cancellationToken)
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
            INNER JOIN "players"."PlayerAuthIdentities" AS "LookupAuth"
                ON "LookupAuth"."PlayerId" = "Player"."Id"
            WHERE "LookupAuth"."Provider" = @Provider
              AND "LookupAuth"."ExternalId" = @ExternalId;

            SELECT
                "Auth"."Provider"   AS "Provider",
                "Auth"."ExternalId" AS "ExternalId",
                "Auth"."Email"      AS "Email",
                "Auth"."LinkedAt"   AS "LinkedAt"
            FROM "players"."PlayerAuthIdentities" AS "Auth"
            INNER JOIN "players"."PlayerAuthIdentities" AS "LookupAuth"
                ON "LookupAuth"."PlayerId" = "Auth"."PlayerId"
            WHERE "LookupAuth"."Provider" = @Provider
              AND "LookupAuth"."ExternalId" = @ExternalId
            ORDER BY "Auth"."LinkedAt" ASC;
        """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { Provider = query.Provider.ToString(), query.ExternalId },
                cancellationToken: cancellationToken
            )
        );

        var dto = await multi.ReadSingleOrDefaultAsync<PlayerDetailsDto>();
        if (dto is null)
            return null;

        var authIdentities = (await multi.ReadAsync<AuthIdentityDto>()).ToList();

        return dto with { AuthIdentities = authIdentities };
    }
}
