using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.GameLinks;

public sealed record TargetResolution(LinkId TargetId, IReadOnlyList<LinkId> PathIds);

public interface ITargetLinkResolver
{
    TargetResolution Resolve(LinkId startLinkId, int chainDepth = 3);
}
