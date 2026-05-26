namespace LexiLink.Modules.Undo.Infrastructure.Configuration.Processing;

internal class UndoUnitOfWork
{
    private readonly UndoContext _context;
    private readonly UndoDomainEventsDispatcher _domainEventsDispatcher;

    internal UndoUnitOfWork(UndoContext context, UndoDomainEventsDispatcher domainEventsDispatcher)
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
