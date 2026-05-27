using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Market.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly MarketContext _marketContext;

    internal OutboxAccessor(MarketContext marketContext)
    {
        _marketContext = marketContext;
    }

    public void Add(OutboxMessage message) => _marketContext.Set<OutboxMessage>().Add(message);
}
