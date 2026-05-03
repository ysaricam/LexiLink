using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Links;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Links;

internal class LinkRepository : ILinkRepository
{
    private readonly GamesContext _gamesContext;

    internal LinkRepository(GamesContext gamesContext)
    {
        _gamesContext = gamesContext;
    }

    public async Task<Link?> GetByIdAsync(LinkId id, CancellationToken cancellationToken = default)
    {
        return await _gamesContext.Links.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<LinkId>> GetIdsByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _gamesContext.Links
            .Where(x => EF.Property<CategoryId>(x, "_categoryId") == categoryId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LinkId>> GetActiveIdsByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _gamesContext.Links
            .Where(x => EF.Property<CategoryId>(x, "_categoryId") == categoryId
                     && EF.Property<bool>(x, "_isActive"))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Link link, CancellationToken cancellationToken = default)
    {
        await _gamesContext.Links.AddAsync(link, cancellationToken);
    }
}
