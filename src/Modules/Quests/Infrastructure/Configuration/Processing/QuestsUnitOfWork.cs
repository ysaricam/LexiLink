namespace LexiLink.Modules.Quests.Infrastructure.Configuration.Processing;

internal class QuestsUnitOfWork
{
    private readonly QuestsContext _context;
    private readonly QuestsDomainEventsDispatcher _domainEventsDispatcher;

    internal QuestsUnitOfWork(QuestsContext context, QuestsDomainEventsDispatcher domainEventsDispatcher)
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
