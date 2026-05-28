using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Payments.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly PaymentsContext _paymentsContext;

    internal OutboxAccessor(PaymentsContext paymentsContext)
    {
        _paymentsContext = paymentsContext;
    }

    public void Add(OutboxMessage message) => _paymentsContext.Set<OutboxMessage>().Add(message);
}
