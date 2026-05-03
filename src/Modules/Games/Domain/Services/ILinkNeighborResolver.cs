using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Services;

public interface ILinkNeighborResolver
{
    IReadOnlyCollection<LinkId> GetOutgoingLinkIds(LinkId linkId);
}
