using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Domain.Links;

public interface ILinkRepository : IRepository<Link>
{
    Task<Link?> GetByIdAsync(LinkId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LinkId>> GetIdsByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LinkId>> GetActiveIdsByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Link link, CancellationToken cancellationToken = default);
}
