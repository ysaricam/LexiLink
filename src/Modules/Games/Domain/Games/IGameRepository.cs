namespace LexiLink.Modules.Games.Domain.Games;

public interface IGameRepository
{
    Task AddAsync(Game game);
    
    Task<Game?> GetByIdAsync(GameId id);
    
    Task UpdateAsync(Game game);
}