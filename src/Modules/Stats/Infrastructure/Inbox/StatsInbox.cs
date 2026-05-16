using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;
using Newtonsoft.Json;

namespace LexiLink.Modules.Stats.Infrastructure.Inbox;

internal sealed class StatsInbox : IStatsInbox
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal StatsInbox(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateNewConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO "stats"."InboxMessages" ("Id", "OccurredOn", "Type", "Data")
            VALUES (@Id, @OccurredOn, @Type, @Data)
            ON CONFLICT ("Id") DO NOTHING
            """,
            new
            {
                integrationEvent.Id,
                integrationEvent.OccurredOn,
                Type = StatsInboxMessageTypeMap.GetName(integrationEvent),
                Data = JsonConvert.SerializeObject(integrationEvent)
            });
    }
}
