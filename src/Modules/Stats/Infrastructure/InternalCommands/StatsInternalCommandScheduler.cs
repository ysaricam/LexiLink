using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using Newtonsoft.Json;

namespace LexiLink.Modules.Stats.Infrastructure.InternalCommands;

internal sealed class StatsInternalCommandScheduler : IStatsInternalCommandScheduler
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IClock _clock;

    internal StatsInternalCommandScheduler(ISqlConnectionFactory sqlConnectionFactory, IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _clock = clock;
    }

    public async Task ScheduleAsync(
        IInternalCommand command,
        DateTime? dueDate = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _sqlConnectionFactory.CreateNewConnection();
        var now = _clock.UtcNow;

        await connection.ExecuteAsync(
            """
            INSERT INTO "stats"."InternalCommands" ("Id", "EnqueueDate", "DueDate", "Type", "Data")
            VALUES (@Id, @EnqueueDate, @DueDate, @Type, @Data)
            ON CONFLICT ("Id") DO NOTHING
            """,
            new
            {
                command.Id,
                EnqueueDate = now,
                DueDate = dueDate ?? now,
                Type = StatsInternalCommandTypeMap.GetName(command),
                Data = JsonConvert.SerializeObject(command)
            });
    }
}
