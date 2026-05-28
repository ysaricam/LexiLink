using LexiLink.Common.Infrastructure;

namespace LexiLink.Modules.Payments.Infrastructure.Configuration.Processing;

internal class PaymentsUnitOfWork : IUnitOfWork
{
    private readonly PaymentsContext _context;
    private readonly PaymentsDomainEventsDispatcher _domainEventsDispatcher;

    internal PaymentsUnitOfWork(
        PaymentsContext context,
        PaymentsDomainEventsDispatcher domainEventsDispatcher)
    {
        _context = context;
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        await _domainEventsDispatcher.DispatchEventsAsync(cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
