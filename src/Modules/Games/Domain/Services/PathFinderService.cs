using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Services;

public sealed class PathFinderService : IPathFinderService
{
    private readonly ILinkNeighborResolver _neighborResolver;

    public PathFinderService(ILinkNeighborResolver neighborResolver)
    {
        _neighborResolver = neighborResolver;
    }

    public LinkId? FindTarget(
        LinkId startLinkId,
        IReadOnlyList<LinkId> categoryLinkIds,
        int minDepth,
        int maxDepth)
    {
        var allowed = new HashSet<LinkId>(categoryLinkIds);
        if (!allowed.Contains(startLinkId))
        {
            return null;
        }

        var visited = new HashSet<LinkId> { startLinkId };
        var queue = new Queue<(LinkId Id, int Depth)>();
        queue.Enqueue((startLinkId, 0));

        while (queue.Count > 0)
        {
            var (id, depth) = queue.Dequeue();

            if (depth >= minDepth && depth <= maxDepth && id != startLinkId)
            {
                return id;
            }

            if (depth >= maxDepth)
            {
                continue;
            }

            foreach (var neighborId in _neighborResolver.GetOutgoingLinkIds(id))
            {
                if (allowed.Contains(neighborId) && visited.Add(neighborId))
                {
                    queue.Enqueue((neighborId, depth + 1));
                }
            }
        }

        return null;
    }

    public IReadOnlyList<LinkId> FindOptimalPath(LinkId startLinkId, LinkId targetLinkId)
    {
        if (startLinkId == targetLinkId)
        {
            return [];
        }

        var parents = new Dictionary<LinkId, LinkId> { [startLinkId] = startLinkId };
        var queue = new Queue<LinkId>();
        queue.Enqueue(startLinkId);

        var found = false;
        while (queue.Count > 0 && !found)
        {
            var id = queue.Dequeue();
            foreach (var neighborId in _neighborResolver.GetOutgoingLinkIds(id))
            {
                if (parents.ContainsKey(neighborId))
                {
                    continue;
                }

                parents[neighborId] = id;

                if (neighborId == targetLinkId)
                {
                    found = true;
                    break;
                }

                queue.Enqueue(neighborId);
            }
        }

        if (!found)
        {
            return [];
        }

        var path = new List<LinkId>();
        var current = targetLinkId;
        while (current != startLinkId)
        {
            path.Add(current);
            current = parents[current];
        }
        path.Reverse();
        return path;
    }
}
