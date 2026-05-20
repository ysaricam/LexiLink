using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Administration.Application.Configuration.Queries;

namespace LexiLink.Modules.Administration.Application.AdminActions.GetAdminActions;

internal sealed class GetAdminActionsQueryHandler
    : IQueryHandler<GetAdminActionsQuery, IReadOnlyList<AdminActionDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetAdminActionsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<AdminActionDto>> Handle(
        GetAdminActionsQuery request,
        CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.GetOpenConnection();

        // Dynamic WHERE assembled from optional filters. Each filter is
        // bound with a parameter, so SQL injection isn't a concern.
        var sql = """
            SELECT
                "Id"          AS "Id",
                "OccurredOn"  AS "OccurredOn",
                "AdminUserId" AS "AdminUserId",
                "ActionType"  AS "ActionType",
                "TargetType"  AS "TargetType",
                "TargetId"    AS "TargetId",
                "PayloadJson" AS "PayloadJson"
            FROM "administration"."AdminActionAudit"
            WHERE 1 = 1
            """;

        var parameters = new DynamicParameters();
        if (request.AdminUserId is not null)
        {
            sql += " AND \"AdminUserId\" = @AdminUserId";
            parameters.Add("AdminUserId", request.AdminUserId.Value);
        }
        if (request.TargetType is not null)
        {
            sql += " AND \"TargetType\" = @TargetType";
            parameters.Add("TargetType", request.TargetType);
        }
        if (request.TargetId is not null)
        {
            sql += " AND \"TargetId\" = @TargetId";
            parameters.Add("TargetId", request.TargetId);
        }

        sql += " ORDER BY \"OccurredOn\" DESC OFFSET @Offset LIMIT @Limit";
        parameters.Add("Offset", request.Offset);
        parameters.Add("Limit", request.Limit);

        var rows = await connection.QueryAsync<AdminActionDto>(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));

        return rows.AsList();
    }
}
