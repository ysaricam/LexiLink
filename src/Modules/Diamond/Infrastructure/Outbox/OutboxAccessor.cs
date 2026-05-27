using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Diamond.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly DiamondContext _diamondContext;

    internal OutboxAccessor(DiamondContext diamondContext)
    {
        _diamondContext = diamondContext;
    }

    public void Add(OutboxMessage message) => _diamondContext.Set<OutboxMessage>().Add(message);
}
