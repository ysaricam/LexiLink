using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Queries;

namespace LexiLink.Modules.Games.Application.Games.GetGameById;

internal class GetGameByIdQueryHandler : IQueryHandler<GetGameByIdQuery, GameDetailsDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetGameByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<GameDetailsDto> Handle(GetGameByIdQuery query, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        const string sql = """
            SELECT
                "Game"."Id"             AS "Id",
                "Game"."PlayerId"       AS "PlayerId",
                "Game"."CategoryId"     AS "CategoryId",
                "Game"."Difficulty"     AS "Difficulty",
                "Game"."StartLinkId"    AS "StartLinkId",
                "StartLink"."Value"     AS "StartWord",
                "Game"."TargetLinkId"   AS "TargetLinkId",
                "TargetLink"."Value"    AS "TargetWord",
                "TargetLink"."Description" AS "TargetDescription",
                "Game"."CurrentLinkId"  AS "CurrentLinkId",
                "CurrentLink"."Value"   AS "CurrentWord",
                "CurrentLink"."Description" AS "CurrentDescription",
                "Game"."State"          AS "State",
                "Game"."Score"          AS "Score",
                "Game"."MaxSteps"       AS "MaxSteps",
                "Game"."StepsTaken"     AS "StepsTaken",
                "Game"."HintsTotal"     AS "HintsTotal",
                "Game"."HintsUsed"      AS "HintsUsed",
                "Game"."UndosTotal"     AS "UndosTotal",
                "Game"."UndosUsed"      AS "UndosUsed",
                "Game"."ResetsTotal"    AS "ResetsTotal",
                "Game"."ResetsUsed"     AS "ResetsUsed"
            FROM "games"."v_Games" AS "Game"
            INNER JOIN "games"."v_Links" AS "StartLink"   ON "StartLink"."Id"   = "Game"."StartLinkId"
            INNER JOIN "games"."v_Links" AS "TargetLink"  ON "TargetLink"."Id"  = "Game"."TargetLinkId"
            INNER JOIN "games"."v_Links" AS "CurrentLink" ON "CurrentLink"."Id" = "Game"."CurrentLinkId"
            WHERE "Game"."Id" = @GameId;

            SELECT
                "History"."StepNumber" AS "StepNumber",
                "History"."LinkId"     AS "LinkId",
                "Link"."Value"         AS "LinkValue"
            FROM "games"."v_GameHistory" AS "History"
            INNER JOIN "games"."v_Links" AS "Link" ON "Link"."Id" = "History"."LinkId"
            WHERE "History"."GameId" = @GameId
            ORDER BY "History"."StepNumber" ASC;
        """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { query.GameId },
                cancellationToken: cancellationToken
            )
        );

        var dto = await multi.ReadSingleOrDefaultAsync<GameDetailsDto>()
            ?? throw new NotFoundException(nameof(GameDetailsDto), query.GameId);

        var history = (await multi.ReadAsync<GameHistoryStepDto>()).ToList();

        return dto with { History = history };
    }
}
