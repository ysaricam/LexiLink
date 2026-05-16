namespace LexiLink.Modules.Players.Infrastructure.Configuration.Processing;

internal class PlayersUnitOfWork
{
    private readonly PlayersContext _context;
    private readonly PlayersDomainEventsDispatcher _domainEventsDispatcher;

    internal PlayersUnitOfWork(PlayersContext context, PlayersDomainEventsDispatcher domainEventsDispatcher)
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
