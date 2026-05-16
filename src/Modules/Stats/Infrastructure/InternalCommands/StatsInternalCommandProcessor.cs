using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Application.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LexiLink.Modules.Stats.Infrastructure.InternalCommands;

internal sealed class StatsInternalCommandProcessor : IStatsInternalCommandProcessor
{
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan RetryBackoff = TimeSpan.FromSeconds(30);

    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ISender _sender;
    private readonly ILogger<StatsInternalCommandProcessor> _logger;
    private readonly IClock _clock;

    internal StatsInternalCommandProcessor(
        ISqlConnectionFactory sqlConnectionFactory,
        ISender sender,
        ILogger<StatsInternalCommandProcessor> logger,
        IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _sender = sender;
        _logger = logger;
        _clock = clock;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProcessorQueue"] = "stats-internal-commands",
            ["ProcessorType"] = GetType().FullName ?? nameof(StatsInternalCommandProcessor)
        });

        using var connection = _sqlConnectionFactory.CreateNewConnection();
        var now = _clock.UtcNow;

        var commands = (await connection.QueryAsync<InternalCommandDto>(
            """
            SELECT "Id", "Type", "Data", "RetryCount"
            FROM "stats"."InternalCommands"
            WHERE "ProcessedDate" IS NULL
              AND "DueDate" <= @Now
              AND "RetryCount" < @MaxRetryCount
              AND ("NextRetryDate" IS NULL OR "NextRetryDate" <= @Now)
            ORDER BY "DueDate", "EnqueueDate"
            LIMIT 100
            """,
            new { Now = now, MaxRetryCount })).AsList();

        foreach (var command in commands)
        {
            try
            {
                var request = Deserialize(command);
                await _sender.Send(request, cancellationToken);

                await connection.ExecuteAsync(
                    """
                    UPDATE "stats"."InternalCommands"
                    SET "ProcessedDate" = @ProcessedDate,
                        "NextRetryDate" = NULL,
                        "Error" = NULL
                    WHERE "Id" = @Id
                    """,
                    new { command.Id, ProcessedDate = _clock.UtcNow });
            }
            catch (Exception ex)
            {
                var retryCount = command.RetryCount + 1;
                var error = ex.ToString();
                if (error.Length > 4000)
                {
                    error = error[..4000];
                }

                await connection.ExecuteAsync(
                    """
                    UPDATE "stats"."InternalCommands"
                    SET "RetryCount" = @RetryCount,
                        "NextRetryDate" = @NextRetryDate,
                        "Error" = @Error
                    WHERE "Id" = @Id
                    """,
                    new
                    {
                        command.Id,
                        RetryCount = retryCount,
                        NextRetryDate = _clock.UtcNow.Add(RetryBackoff),
                        Error = error
                    });

                _logger.LogError(
                    ex,
                    "Failed to process Stats internal command {InternalCommandId}. Retry {RetryCount}.",
                    command.Id,
                    retryCount);
            }
        }
    }

    private static ICommand Deserialize(InternalCommandDto command)
    {
        var type = StatsInternalCommandTypeMap.GetType(command.Type);
        if (type is null)
        {
            throw new ApplicationException($"Stats internal command type '{command.Type}' is not mapped.");
        }

        var deserializedCommand = JsonConvert.DeserializeObject(command.Data, type);
        if (deserializedCommand is not ICommand typedCommand)
        {
            throw new ApplicationException($"Stats internal command type '{command.Type}' is not a command.");
        }

        return typedCommand;
    }

    private sealed record InternalCommandDto(Guid Id, string Type, string Data, int RetryCount);
}
