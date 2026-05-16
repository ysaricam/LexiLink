using LexiLink.Common.Infrastructure.Outbox;
using LexiLink.Common.Application.Time;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LexiLink.API.Configuration.Operations;

public sealed class ProcessorBacklogReader
{
    private const int StatsProcessorMaxRetryCount = 10;
    private const int ErrorLimit = 5;

    private readonly string _connectionString;
    private readonly int _outboxMaxRetryCount;
    private readonly IClock _clock;

    public ProcessorBacklogReader(
        string connectionString,
        IOptions<OutboxProcessingOptions> outboxOptions,
        IClock clock)
    {
        _connectionString = connectionString;
        _outboxMaxRetryCount = outboxOptions.Value.MaxRetryCount;
        _clock = clock;
    }

    public async Task<ProcessorBacklogResponse> ReadAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var now = _clock.UtcNow;
        var queues = new List<ProcessorQueueBacklog>
        {
            await ReadOccurredOnQueueAsync(
                connection,
                name: "games-outbox",
                module: "Games",
                kind: "Outbox",
                schema: "games",
                table: "OutboxMessages",
                maxRetryCount: _outboxMaxRetryCount,
                now,
                cancellationToken),
            await ReadOccurredOnQueueAsync(
                connection,
                name: "players-outbox",
                module: "Players",
                kind: "Outbox",
                schema: "players",
                table: "OutboxMessages",
                maxRetryCount: _outboxMaxRetryCount,
                now,
                cancellationToken),
            await ReadOccurredOnQueueAsync(
                connection,
                name: "stats-inbox",
                module: "Stats",
                kind: "Inbox",
                schema: "stats",
                table: "InboxMessages",
                maxRetryCount: StatsProcessorMaxRetryCount,
                now,
                cancellationToken),
            await ReadInternalCommandQueueAsync(connection, now, cancellationToken)
        };

        return new ProcessorBacklogResponse(now, queues);
    }

    private static async Task<ProcessorQueueBacklog> ReadOccurredOnQueueAsync(
        NpgsqlConnection connection,
        string name,
        string module,
        string kind,
        string schema,
        string table,
        int maxRetryCount,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var summary = await ReadSummaryAsync(
            connection,
            schema,
            table,
            "\"OccurredOn\"",
            extraReadyPredicate: null,
            maxRetryCount,
            now,
            cancellationToken);
        var errors = await ReadErrorsAsync(connection, schema, table, maxRetryCount, cancellationToken);

        return new ProcessorQueueBacklog(
            name,
            module,
            kind,
            maxRetryCount,
            summary.TotalUnprocessed,
            summary.ReadyToProcess,
            summary.RetryScheduled,
            summary.Poisoned,
            summary.Failed,
            summary.OldestUnprocessedOn,
            errors);
    }

    private static async Task<ProcessorQueueBacklog> ReadInternalCommandQueueAsync(
        NpgsqlConnection connection,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var summary = await ReadSummaryAsync(
            connection,
            schema: "stats",
            table: "InternalCommands",
            orderingColumn: "\"DueDate\"",
            extraReadyPredicate: "\"DueDate\" <= @Now",
            maxRetryCount: StatsProcessorMaxRetryCount,
            now,
            cancellationToken);
        var errors = await ReadErrorsAsync(
            connection,
            schema: "stats",
            table: "InternalCommands",
            StatsProcessorMaxRetryCount,
            cancellationToken);

        return new ProcessorQueueBacklog(
            "stats-internal-commands",
            "Stats",
            "InternalCommands",
            StatsProcessorMaxRetryCount,
            summary.TotalUnprocessed,
            summary.ReadyToProcess,
            summary.RetryScheduled,
            summary.Poisoned,
            summary.Failed,
            summary.OldestUnprocessedOn,
            errors);
    }

    private static async Task<QueueSummaryRow> ReadSummaryAsync(
        NpgsqlConnection connection,
        string schema,
        string table,
        string orderingColumn,
        string? extraReadyPredicate,
        int maxRetryCount,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var readyPredicate = extraReadyPredicate is null
            ? string.Empty
            : $"AND {extraReadyPredicate}";

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT
                COUNT(*) FILTER (WHERE "ProcessedDate" IS NULL) AS total_unprocessed,
                COUNT(*) FILTER (
                    WHERE "ProcessedDate" IS NULL
                      AND "RetryCount" < @MaxRetryCount
                      AND ("NextRetryDate" IS NULL OR "NextRetryDate" <= @Now)
                      {readyPredicate}
                ) AS ready_to_process,
                COUNT(*) FILTER (
                    WHERE "ProcessedDate" IS NULL
                      AND "RetryCount" < @MaxRetryCount
                      AND "NextRetryDate" > @Now
                ) AS retry_scheduled,
                COUNT(*) FILTER (
                    WHERE "ProcessedDate" IS NULL
                      AND "RetryCount" >= @MaxRetryCount
                ) AS poisoned,
                COUNT(*) FILTER (
                    WHERE "ProcessedDate" IS NULL
                      AND "Error" IS NOT NULL
                ) AS failed,
                MIN({orderingColumn}) FILTER (WHERE "ProcessedDate" IS NULL) AS oldest_unprocessed_on
            FROM "{schema}"."{table}"
            """;
        command.Parameters.AddWithValue("MaxRetryCount", maxRetryCount);
        command.Parameters.AddWithValue("Now", now);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new QueueSummaryRow(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2),
            reader.GetInt64(3),
            reader.GetInt64(4),
            reader.IsDBNull(5) ? null : reader.GetDateTime(5));
    }

    private static async Task<IReadOnlyList<ProcessorQueueErrorSample>> ReadErrorsAsync(
        NpgsqlConnection connection,
        string schema,
        string table,
        int maxRetryCount,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT "Id", "Type", "RetryCount", "NextRetryDate", "Error"
            FROM "{schema}"."{table}"
            WHERE "ProcessedDate" IS NULL
              AND "Error" IS NOT NULL
            ORDER BY
                ("RetryCount" >= @MaxRetryCount) DESC,
                "RetryCount" DESC,
                "NextRetryDate" DESC NULLS LAST
            LIMIT @ErrorLimit
            """;
        command.Parameters.AddWithValue("MaxRetryCount", maxRetryCount);
        command.Parameters.AddWithValue("ErrorLimit", ErrorLimit);

        var errors = new List<ProcessorQueueErrorSample>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            errors.Add(new ProcessorQueueErrorSample(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                TrimError(reader.GetString(4))));
        }

        return errors;
    }

    private static string TrimError(string error) =>
        error.Length <= 500 ? error : error[..500];

    private sealed record QueueSummaryRow(
        long TotalUnprocessed,
        long ReadyToProcess,
        long RetryScheduled,
        long Poisoned,
        long Failed,
        DateTime? OldestUnprocessedOn);
}

public sealed record ProcessorBacklogResponse(
    DateTime CheckedAt,
    IReadOnlyList<ProcessorQueueBacklog> Queues);

public sealed record ProcessorQueueBacklog(
    string Name,
    string Module,
    string Kind,
    int MaxRetryCount,
    long TotalUnprocessed,
    long ReadyToProcess,
    long RetryScheduled,
    long Poisoned,
    long Failed,
    DateTime? OldestUnprocessedOn,
    IReadOnlyList<ProcessorQueueErrorSample> Errors);

public sealed record ProcessorQueueErrorSample(
    Guid Id,
    string Type,
    int RetryCount,
    DateTime? NextRetryDate,
    string Error);
