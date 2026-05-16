namespace LexiLink.Modules.Energy.Infrastructure.Configuration.Processing;

internal class EnergyUnitOfWork
{
    private readonly EnergyContext _context;
    private readonly EnergyDomainEventsDispatcher _domainEventsDispatcher;

    internal EnergyUnitOfWork(EnergyContext context, EnergyDomainEventsDispatcher domainEventsDispatcher)
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
