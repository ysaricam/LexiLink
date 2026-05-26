using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Reset.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly ResetContext _resetContext;

    internal OutboxAccessor(ResetContext resetContext)
    {
        _resetContext = resetContext;
    }

    public void Add(OutboxMessage message) => _resetContext.Set<OutboxMessage>().Add(message);
}
