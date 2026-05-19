using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Administration.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly AdministrationContext _administrationContext;

    internal OutboxAccessor(AdministrationContext administrationContext)
    {
        _administrationContext = administrationContext;
    }

    public void Add(OutboxMessage message) => _administrationContext.Set<OutboxMessage>().Add(message);
}
