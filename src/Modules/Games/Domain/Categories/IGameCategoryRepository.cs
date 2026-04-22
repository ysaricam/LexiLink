namespace LexiLink.Modules.Games.Domain.Categories;

public interface IGameCategoryRepository
{
    Task AddAsync(GameCategory category);
    
    Task<GameCategory?> GetByIdAsync(GameCategoryId id);
}