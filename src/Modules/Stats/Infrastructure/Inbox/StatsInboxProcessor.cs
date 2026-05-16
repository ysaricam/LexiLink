using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LexiLink.Modules.Stats.Infrastructure.Inbox;

internal sealed class StatsInboxProcessor : IStatsInboxProcessor
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IPlayerStatsProjectionUpdater _projectionUpdater;
    private readonly ILogger<StatsInboxProcessor> _logger;
    private readonly IClock _clock;

    internal StatsInboxProcessor(
        ISqlConnectionFactory sqlConnectionFactory,
        IPlayerStatsProjectionUpdater projectionUpdater,
        ILogger<StatsInboxProcessor> logger,
        IClock clock)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _projectionUpdater = projectionUpdater;
        _logger = logger;
        _clock = clock;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProcessorQueue"] = "stats-inbox",
            ["ProcessorType"] = GetType().FullName ?? nameof(StatsInboxProcessor)
        });

        using var connection = _sqlConnectionFactory.CreateNewConnection();
        var now = _clock.UtcNow;

        var messages = (await connection.QueryAsync<InboxMessageDto>(
            """
            SELECT "Id", "Type", "Data", "RetryCount"
            FROM "stats"."InboxMessages"
            WHERE "ProcessedDate" IS NULL
              AND "RetryCount" < 10
              AND ("NextRetryDate" IS NULL OR "NextRetryDate" <= @Now)
            ORDER BY "OccurredOn"
            LIMIT 100
            """,
            new { Now = now })).AsList();

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = Deserialize(message);
                await ProjectAsync(integrationEvent, cancellationToken);

                await connection.ExecuteAsync(
                    """
                    UPDATE "stats"."InboxMessages"
                    SET "ProcessedDate" = @ProcessedDate,
                        "NextRetryDate" = NULL,
                        "Error" = NULL
                    WHERE "Id" = @Id
                    """,
                    new { message.Id, ProcessedDate = _clock.UtcNow });
            }
            catch (Exception ex)
            {
                var retryCount = message.RetryCount + 1;
                var error = ex.ToString();
                if (error.Length > 4000)
                {
                    error = error[..4000];
                }

                await connection.ExecuteAsync(
                    """
                    UPDATE "stats"."InboxMessages"
                    SET "RetryCount" = @RetryCount,
                        "NextRetryDate" = @NextRetryDate,
                        "Error" = @Error
                    WHERE "Id" = @Id
                    """,
                    new
                    {
                        message.Id,
                        RetryCount = retryCount,
                        NextRetryDate = _clock.UtcNow.AddSeconds(30),
                        Error = error
                    });

                _logger.LogError(
                    ex,
                    "Failed to process Stats inbox message {InboxMessageId}. Retry {RetryCount}.",
                    message.Id,
                    retryCount);
            }
        }
    }

    private static IIntegrationEvent Deserialize(InboxMessageDto message)
    {
        var type = StatsInboxMessageTypeMap.GetType(message.Type);
        if (type is null)
        {
            throw new ApplicationException($"Stats inbox message type '{message.Type}' is not mapped.");
        }

        var integrationEvent = JsonConvert.DeserializeObject(message.Data, type);
        if (integrationEvent is not IIntegrationEvent typedIntegrationEvent)
        {
            throw new ApplicationException($"Stats inbox message type '{message.Type}' is not an integration event.");
        }

        return typedIntegrationEvent;
    }

    private Task ProjectAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken) =>
        integrationEvent switch
        {
            PlayerRegisteredIntegrationEvent playerRegistered =>
                _projectionUpdater.ProjectAsync(playerRegistered, cancellationToken),
            AuthProviderLinkedIntegrationEvent authProviderLinked =>
                _projectionUpdater.ProjectAsync(authProviderLinked, cancellationToken),
            PlayerProfileUpdatedIntegrationEvent playerProfileUpdated =>
                _projectionUpdater.ProjectAsync(playerProfileUpdated, cancellationToken),
            GameCompletedIntegrationEvent gameCompleted =>
                _projectionUpdater.ProjectAsync(gameCompleted, cancellationToken),
            _ => throw new ApplicationException(
                $"Stats inbox message type '{integrationEvent.GetType().FullName}' is not supported.")
        };

    private sealed record InboxMessageDto(Guid Id, string Type, string Data, int RetryCount);
}
