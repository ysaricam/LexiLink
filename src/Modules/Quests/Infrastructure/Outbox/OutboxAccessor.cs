using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Quests.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly QuestsContext _questsContext;

    internal OutboxAccessor(QuestsContext questsContext)
    {
        _questsContext = questsContext;
    }

    public void Add(OutboxMessage message) => _questsContext.Set<OutboxMessage>().Add(message);
}
