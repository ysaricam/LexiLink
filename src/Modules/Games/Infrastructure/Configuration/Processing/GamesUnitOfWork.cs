namespace LexiLink.Modules.Games.Infrastructure.Configuration.Processing;

internal class GamesUnitOfWork
{
    private readonly GamesContext _context;
    private readonly GamesDomainEventsDispatcher _domainEventsDispatcher;

    internal GamesUnitOfWork(GamesContext context, GamesDomainEventsDispatcher domainEventsDispatcher)
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
