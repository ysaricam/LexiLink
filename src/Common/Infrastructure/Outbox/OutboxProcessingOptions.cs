namespace LexiLink.Common.Infrastructure.Outbox;

public sealed class OutboxProcessingOptions
{
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);

    public int MaxRetryCount { get; init; } = 10;

    public TimeSpan RetryBackoff { get; init; } = TimeSpan.FromSeconds(30);
}
