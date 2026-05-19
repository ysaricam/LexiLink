namespace LexiLink.Modules.Administration.Infrastructure.Configuration.Processing;

internal class AdministrationUnitOfWork
{
    private readonly AdministrationContext _context;
    private readonly AdministrationDomainEventsDispatcher _domainEventsDispatcher;

    internal AdministrationUnitOfWork(
        AdministrationContext context,
        AdministrationDomainEventsDispatcher domainEventsDispatcher)
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
