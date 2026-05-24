namespace LexiLink.Modules.Hint.Infrastructure.Configuration.Processing;

internal class HintUnitOfWork
{
    private readonly HintContext _context;
    private readonly HintDomainEventsDispatcher _domainEventsDispatcher;

    internal HintUnitOfWork(HintContext context, HintDomainEventsDispatcher domainEventsDispatcher)
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
