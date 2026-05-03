namespace LexiLink.Common.Application.Outbox;

public class OutboxMessage
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string Type { get; }
    public string Data { get; }
    public DateTime? ProcessedDate { get; private set; }

    public OutboxMessage(Guid id, DateTime occurredOn, string type, string data)
    {
        Id = id;
        OccurredOn = occurredOn;
        Type = type;
        Data = data;
    }

    private OutboxMessage()
    { }

    public void Process()
    {
        ProcessedDate = DateTime.UtcNow;
    }
}
