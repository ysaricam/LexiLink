using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Services;

public interface IPathFinderService
{
    LinkId? FindTarget(
        LinkId startLinkId,
        IReadOnlyList<LinkId> categoryLinkIds,
        int minDepth,
        int maxDepth);
    
    IReadOnlyList<LinkId> FindOptimalPath(
        LinkId startLinkId,
        LinkId targetLinkId);
    
}