using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Energy.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly EnergyContext _energyContext;

    internal OutboxAccessor(EnergyContext energyContext)
    {
        _energyContext = energyContext;
    }

    public void Add(OutboxMessage message) => _energyContext.Set<OutboxMessage>().Add(message);
}
