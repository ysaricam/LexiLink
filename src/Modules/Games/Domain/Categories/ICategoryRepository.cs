using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
