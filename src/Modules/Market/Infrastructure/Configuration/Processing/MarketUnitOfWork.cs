namespace LexiLink.Modules.Market.Infrastructure.Configuration.Processing;

internal class MarketUnitOfWork
{
    private readonly MarketContext _context;
    private readonly MarketDomainEventsDispatcher _domainEventsDispatcher;

    internal MarketUnitOfWork(MarketContext context, MarketDomainEventsDispatcher domainEventsDispatcher)
    {
        _context = context;
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        await _domainEventsDispatcher.DispatchEventsAsync();

        return await _context.SaveChangesAsync(cancellationToken);
    }
}
