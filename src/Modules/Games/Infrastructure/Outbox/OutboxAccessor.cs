using LexiLink.Common.Application.Outbox;

namespace LexiLink.Modules.Games.Infrastructure.Outbox;

internal class OutboxAccessor : IOutbox
{
    private readonly GamesContext _gamesContext;

    internal OutboxAccessor(GamesContext gamesContext)
    {
        _gamesContext = gamesContext;
    }

    public void Add(OutboxMessage message) => _gamesContext.Set<OutboxMessage>().Add(message);
}
