using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Administration.Application.AdminActions.Projection;
using LexiLink.Modules.Administration.IntegrationEvents;

namespace LexiLink.Modules.Administration.Infrastructure.AdminActions;

internal sealed class AdminActionAuditWriter : IAdminActionAuditWriter
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal AdminActionAuditWriter(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task AppendAsync(
        AdminActionPerformedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateNewConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO "administration"."AdminActionAudit"
                ("Id", "OccurredOn", "AdminUserId", "ActionType", "TargetType", "TargetId", "PayloadJson")
            VALUES
                (@Id, @OccurredOn, @AdminUserId, @ActionType, @TargetType, @TargetId, @PayloadJson)
            ON CONFLICT ("Id") DO NOTHING;
            """,
            new
            {
                @event.Id,
                @event.OccurredOn,
                @event.AdminUserId,
                @event.ActionType,
                @event.TargetType,
                @event.TargetId,
                @event.PayloadJson
            },
            cancellationToken: cancellationToken));
    }
}
