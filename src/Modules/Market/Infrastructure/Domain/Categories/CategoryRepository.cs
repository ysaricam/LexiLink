using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Market.Infrastructure.Domain.Categories;

internal class CategoryRepository : ICategoryRepository
{
    private readonly MarketContext _marketContext;

    internal CategoryRepository(MarketContext marketContext)
    {
        _marketContext = marketContext;
    }

    public async Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default)
    {
        return await _marketContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _marketContext.Categories.AddAsync(category, cancellationToken);
    }
}
