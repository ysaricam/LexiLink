using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Ads.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly AdsContext _adsContext;

    internal OutboxAccessor(AdsContext adsContext)
    {
        _adsContext = adsContext;
    }

    public void Add(OutboxMessage message) => _adsContext.Set<OutboxMessage>().Add(message);
}
