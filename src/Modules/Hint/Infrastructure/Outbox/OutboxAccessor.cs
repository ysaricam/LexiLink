using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Hint.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly HintContext _hintContext;

    internal OutboxAccessor(HintContext hintContext)
    {
        _hintContext = hintContext;
    }

    public void Add(OutboxMessage message) => _hintContext.Set<OutboxMessage>().Add(message);
}
