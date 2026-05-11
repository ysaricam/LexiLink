using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Players.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly PlayersContext _playersContext;

    internal OutboxAccessor(PlayersContext playersContext)
    {
        _playersContext = playersContext;
    }

    public void Add(OutboxMessage message) => _playersContext.Set<OutboxMessage>().Add(message);
}
