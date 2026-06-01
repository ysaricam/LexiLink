namespace LexiLink.Modules.Ads.Infrastructure.Configuration.Processing;

internal class AdsUnitOfWork
{
    private readonly AdsContext _context;
    private readonly AdsDomainEventsDispatcher _domainEventsDispatcher;

    internal AdsUnitOfWork(AdsContext context, AdsDomainEventsDispatcher domainEventsDispatcher)
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
