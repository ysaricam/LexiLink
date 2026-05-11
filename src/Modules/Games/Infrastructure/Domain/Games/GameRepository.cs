using LexiLink.Modules.Games.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Games;

internal class GameRepository : IGameRepository
{
    private readonly GamesContext _gamesContext;

    internal GameRepository(GamesContext gamesContext)
    {
        _gamesContext = gamesContext;
    }

    public async Task<Game?> GetByIdAsync(GameId id, CancellationToken cancellationToken = default)
    {
        return await _gamesContext.Games.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        await _gamesContext.Games.AddAsync(game, cancellationToken);
    }
}
