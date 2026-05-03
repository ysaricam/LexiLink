using LexiLink.Modules.Games.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Categories;

internal class CategoryRepository : ICategoryRepository
{
    private readonly GamesContext _gamesContext;

    internal CategoryRepository(GamesContext gamesContext)
    {
        _gamesContext = gamesContext;
    }

    public async Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default)
    {
        return await _gamesContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _gamesContext.Categories.AddAsync(category, cancellationToken);
    }
}
