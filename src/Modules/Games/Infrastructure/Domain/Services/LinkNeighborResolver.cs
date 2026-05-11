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
        return link?.OutgoingLinkIds ?? Array.Empty<LinkId>();
    }
}
