namespace LexiLink.Modules.Diamond.Infrastructure.Configuration.Processing;

internal class DiamondUnitOfWork
{
    private readonly DiamondContext _context;
    private readonly DiamondDomainEventsDispatcher _domainEventsDispatcher;

    internal DiamondUnitOfWork(DiamondContext context, DiamondDomainEventsDispatcher domainEventsDispatcher)
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
