namespace LexiLink.Modules.Games.Domain.GameLinks;

public sealed class TargetLinkResolver : ITargetLinkResolver
{
    private readonly Random _random;
    private readonly Func<LinkId, Link> _linkFetcher;

    public TargetLinkResolver(Func<LinkId, Link> linkFetcher, int? seed = null)
    {
        _linkFetcher = linkFetcher ?? throw new ArgumentNullException(nameof(linkFetcher));
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public TargetResolution Resolve(LinkId startLinkId, int chainDepth = 3)
    {
        if(startLinkId is null)
            throw new ArgumentNullException(nameof(startLinkId));
        if(chainDepth < 1)
            throw new ArgumentException("Chain depth must be at least 1.", nameof(chainDepth));
        
        var pathIds = new List<LinkId>();
        var currentLinkId = startLinkId;

        for(int i = 0; i < chainDepth; i++)
        {
            var currentLink = _linkFetcher(currentLinkId);
            if(currentLink.SubLinkIds.Count == 0)
                break;

            var randomIndex = _random.Next(currentLink.SubLinkIds.Count);
            currentLinkId = currentLink.SubLinkIds[randomIndex];
            pathIds.Add(currentLinkId);
        }

        return new TargetResolution(currentLinkId, pathIds);
    }
}
