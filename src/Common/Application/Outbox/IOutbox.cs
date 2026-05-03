namespace LexiLink.Common.Application.Outbox;

public interface IOutbox
{
    void Add(OutboxMessage message);
}
