using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games;

public interface IGameRepository : IRepository<Game>
{
    Task<Game?> GetByIdAsync(GameId id, CancellationToken cancellationToken = default);
    Task AddAsync(Game game, CancellationToken cancellationToken = default);
}
