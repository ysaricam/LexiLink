using Dapper;
using LexiLink.Common.Application.Events;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Common.Infrastructure.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Newtonsoft.Json;

namespace LexiLink.Common.Infrastructure.Outbox;

public class OutboxProcessor : IOutboxProcessor
{
    private readonly string _connectionString;
    private readonly string _schemaName;
    private readonly IDomainNotificationsMapper _domainNotificationsMapper;
    private readonly IPublisher _publisher;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IClock _clock;
    private readonly OutboxProcessingOptions _options;

    public string SchemaName => _schemaName;

    public OutboxProcessor(
        string connectionString,
        string schemaName,
        IDomainNotificationsMapper domainNotificationsMapper,
        IPublisher publisher,
        ILogger<OutboxProcessor> logger,
        IClock clock,
        IOptions<OutboxProcessingOptions>? options = null)
    {
        _connectionString = connectionString;
        _schemaName = schemaName;
        _domainNotificationsMapper = domainNotificationsMapper;
        _publisher = publisher;
        _logger = logger;
        _clock = clock;
        _options = options?.Value ?? new OutboxProcessingOptions();
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProcessorQueue"] = $"{_schemaName}-outbox",
            ["ProcessorType"] = GetType().FullName ?? nameof(OutboxProcessor)
        });

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var now = _clock.UtcNow;
        var messages = (await connection.QueryAsync<OutboxMessageDto>(
            new CommandDefinition(
                $"""
                SELECT "Id", "Type", "Data", "RetryCount"
                FROM "{_schemaName}"."OutboxMessages"
                WHERE "ProcessedDate" IS NULL
                  AND "RetryCount" < @MaxRetryCount
                  AND ("NextRetryDate" IS NULL OR "NextRetryDate" <= @Now)
                ORDER BY "OccurredOn"
                LIMIT 100
                """,
                new { _options.MaxRetryCount, Now = now },
                cancellationToken: cancellationToken))).AsList();

        foreach (var message in messages)
        {
            try
            {
                var notificationType = _domainNotificationsMapper.GetType(message.Type);
                if (notificationType is null)
                {
                    throw new ApplicationException($"Outbox message type '{message.Type}' is not mapped.");
                }

                var notification = JsonConvert.DeserializeObject(
                    message.Data,
                    notificationType,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new AllPropertiesContractResolver()
                    });

                if (notification is not IDomainEventNotification domainNotification)
                {
                    throw new ApplicationException($"Outbox message type '{message.Type}' is not a domain notification.");
                }

                await _publisher.Publish(domainNotification, cancellationToken);

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        $"""
                        UPDATE "{_schemaName}"."OutboxMessages"
                        SET "ProcessedDate" = @ProcessedDate,
                            "NextRetryDate" = NULL,
                            "Error" = NULL
                        WHERE "Id" = @Id
                        """,
                        new { message.Id, ProcessedDate = _clock.UtcNow },
                        cancellationToken: cancellationToken));
            }
            catch (Exception ex)
            {
                var retryCount = message.RetryCount + 1;
                var nextRetryDate = retryCount >= _options.MaxRetryCount
                    ? (DateTime?)null
                    : _clock.UtcNow.Add(_options.RetryBackoff);
                var error = ex.ToString();
                if (error.Length > 4000)
                {
                    error = error[..4000];
                }

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        $"""
                        UPDATE "{_schemaName}"."OutboxMessages"
                        SET "RetryCount" = @RetryCount,
                            "NextRetryDate" = @NextRetryDate,
                            "Error" = @Error
                        WHERE "Id" = @Id
                        """,
                        new
                        {
                            message.Id,
                            RetryCount = retryCount,
                            NextRetryDate = nextRetryDate,
                            Error = error
                        },
                        cancellationToken: cancellationToken));

                _logger.LogError(
                    ex,
                    "Failed to process outbox message {OutboxMessageId} from schema {SchemaName}. Retry {RetryCount}/{MaxRetryCount}.",
                    message.Id,
                    _schemaName,
                    retryCount,
                    _options.MaxRetryCount);
            }
        }
    }

    private sealed record OutboxMessageDto(Guid Id, string Type, string Data, int RetryCount);
}
