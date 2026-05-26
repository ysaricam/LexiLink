using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Undo.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly UndoContext _undoContext;

    internal OutboxAccessor(UndoContext undoContext)
    {
        _undoContext = undoContext;
    }

    public void Add(OutboxMessage message) => _undoContext.Set<OutboxMessage>().Add(message);
}
