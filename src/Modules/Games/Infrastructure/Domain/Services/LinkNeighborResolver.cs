using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Services;

internal class LinkNeighborResolver : ILinkNeighborResolver
{
    private readonly GamesContext _gamesContext;

    internal LinkNeighborResolver(GamesContext gamesContext)
    {
        _gamesContext = gamesContext;
    }

    public IReadOnlyCollection<LinkId> GetOutgoingLinkIds(LinkId linkId)
    {
        var link = _gamesContext.Links.FirstOrDefault(x => x.Id == linkId);
        if (link is null)
        {
            return Array.Empty<LinkId>();
        }

        // Sort neighbors by underlying Guid so BFS tie-breaks deterministically
        // and matches the same ordering used by GetGameOptionsQueryHandler's
        // adjacency BFS. Without this, the hint's first hop and the options
        // panel's locked first hop can diverge when multiple shortest paths
        // tie — leaving the hint pointing to a link that isn't even visible.
        return link.OutgoingLinkIds
            .OrderBy(id => id.Value)
            .ToList();
    }
}
