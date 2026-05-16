namespace LexiLink.Common.Application.Outbox;

public class OutboxMessage
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string Type { get; }
    public string Data { get; }
    public DateTime? ProcessedDate { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryDate { get; private set; }
    public string? Error { get; private set; }

    public OutboxMessage(Guid id, DateTime occurredOn, string type, string data)
    {
        Id = id;
        OccurredOn = occurredOn;
        Type = type;
        Data = data;
    }

    private OutboxMessage()
    {
        Type = null!;
        Data = null!;
    }

    public void Process(DateTime processedDate)
    {
        ProcessedDate = processedDate;
        NextRetryDate = null;
        Error = null;
    }
}
