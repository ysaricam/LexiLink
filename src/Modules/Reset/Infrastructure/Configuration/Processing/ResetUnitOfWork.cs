namespace LexiLink.Modules.Reset.Infrastructure.Configuration.Processing;

internal class ResetUnitOfWork
{
    private readonly ResetContext _context;
    private readonly ResetDomainEventsDispatcher _domainEventsDispatcher;

    internal ResetUnitOfWork(ResetContext context, ResetDomainEventsDispatcher domainEventsDispatcher)
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
