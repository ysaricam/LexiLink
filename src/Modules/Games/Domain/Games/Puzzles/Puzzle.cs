using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Domain.Games.Puzzles;

public sealed class Puzzle : ValueObject
{
    private readonly List<OptimalPathStep> _optimalPath;

    public CategoryId CategoryId { get; }
    public Difficulty Difficulty { get; }
    public LinkId StartLinkId { get; }
    public LinkId TargetLinkId { get; }

    public IReadOnlyList<LinkId> OptimalPath =>
        _optimalPath
            .OrderBy(s => s.Position)
            .Select(s => s.LinkId)
            .ToList()
            .AsReadOnly();
    public int Depth => _optimalPath.Count;

    private Puzzle()
    {
        _optimalPath = [];
    }

    private Puzzle(
        CategoryId categoryId,
        Difficulty difficulty,
        LinkId startLinkId,
        LinkId targetLinkId,
        IEnumerable<LinkId> optimalPath)
    {
        CategoryId = categoryId;
        Difficulty = difficulty;
        StartLinkId = startLinkId;
        TargetLinkId = targetLinkId;
        _optimalPath = optimalPath.Select((id, i) => new OptimalPathStep(i, id)).ToList();
    }

    internal static Puzzle Create(
        CategoryId categoryId,
        IReadOnlyList<LinkId> categoryLinkIds,
        Difficulty difficulty,
        IPathFinderService pathFinder,
        IGameConfigurationService gameConfiguration,
        Random random) =>
        Create(categoryId, categoryLinkIds, difficulty, pathFinder, gameConfiguration, random, []);

    internal static Puzzle Create(
        CategoryId categoryId,
        IReadOnlyList<LinkId> categoryLinkIds,
        Difficulty difficulty,
        IPathFinderService pathFinder,
        IGameConfigurationService gameConfiguration,
        Random random,
        IReadOnlyCollection<CompletedGameLinkPair> completedPairs)
    {
        CheckRule(new CategoryMustHaveEnoughLinksToStartGameRule(categoryLinkIds));

        var (minDepth, maxDepth) = gameConfiguration.ResolveDepthRange(difficulty);
        var completedPairSet = completedPairs
            .Select(pair => (pair.StartLinkId, pair.TargetLinkId))
            .ToHashSet();

        foreach (var (candidateMinDepth, candidateMaxDepth) in ResolveDepthSearchOrder(difficulty, minDepth, maxDepth, random))
        {
            foreach (var startLinkId in Shuffle(categoryLinkIds, random))
            {
                foreach (var targetLinkId in categoryLinkIds.Where(linkId => linkId != startLinkId))
                {
                    if (completedPairSet.Contains((startLinkId, targetLinkId)))
                    {
                        continue;
                    }

                    var optimalPath = pathFinder.FindOptimalPath(startLinkId, targetLinkId);
                    if (optimalPath.Count < candidateMinDepth || optimalPath.Count > candidateMaxDepth)
                    {
                        continue;
                    }

                    return new Puzzle(categoryId, difficulty, startLinkId, targetLinkId, optimalPath);
                }
            }
        }

        CheckRule(new PuzzleTargetLinkMustBeReachableRule(null));
        return null!;
    }

    public HintResult RequestHint(LinkId currentLinkId, ILinkNeighborResolver neighborResolver)
    {
        // Live BFS from currentLink to target. The first hop on the
        // shortest path is the recommendation. This is robust to the
        // player going off the precomputed _optimalPath — we just find
        // the best move from wherever they are now. The same BFS is run
        // by GetGameOptionsQueryHandler's reachability lock, so the
        // returned link is always one of the 6 displayed options.
        if (currentLinkId == TargetLinkId)
        {
            return HintResult.WrongPath(currentLinkId);
        }

        var ordered = _optimalPath.OrderBy(s => s.Position).ToList();
        var onOptimalPathIndex = ordered.FindIndex(s => s.LinkId == currentLinkId);
        var isOnOptimalPath =
            onOptimalPathIndex >= 0 && onOptimalPathIndex < ordered.Count - 1;

        var firstHop = FindFirstHopToTarget(currentLinkId, neighborResolver);
        if (firstHop is not null)
        {
            return isOnOptimalPath
                ? HintResult.CorrectPath(firstHop)
                : HintResult.WrongPath(firstHop);
        }

        // Target unreachable from currentLink (graph disconnected). Fall
        // back to the persisted optimal path's first step so the API
        // still returns *something*, even if the link won't be among
        // the visible 6 options.
        var fallback = ordered.Count > 0 ? ordered[0].LinkId : StartLinkId;
        return HintResult.WrongPath(fallback);
    }

    private LinkId? FindFirstHopToTarget(
        LinkId currentLinkId,
        ILinkNeighborResolver neighborResolver)
    {
        var firstHopByNode = new Dictionary<LinkId, LinkId>();
        var queue = new Queue<LinkId>();

        foreach (var neighbor in neighborResolver.GetOutgoingLinkIds(currentLinkId))
        {
            if (firstHopByNode.ContainsKey(neighbor))
            {
                continue;
            }
            firstHopByNode[neighbor] = neighbor;
            if (neighbor == TargetLinkId)
            {
                return neighbor;
            }
            queue.Enqueue(neighbor);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in neighborResolver.GetOutgoingLinkIds(current))
            {
                if (neighbor == currentLinkId || firstHopByNode.ContainsKey(neighbor))
                {
                    continue;
                }
                firstHopByNode[neighbor] = firstHopByNode[current];
                if (neighbor == TargetLinkId)
                {
                    return firstHopByNode[neighbor];
                }
                queue.Enqueue(neighbor);
            }
        }

        return null;
    }

    private static IReadOnlyList<LinkId> Shuffle(IReadOnlyList<LinkId> linkIds, Random random) =>
        linkIds.OrderBy(_ => random.Next()).ToList();

    private static IReadOnlyList<(int MinDepth, int MaxDepth)> ResolveDepthSearchOrder(
        Difficulty difficulty,
        int minDepth,
        int maxDepth,
        Random random)
    {
        if (difficulty != Difficulty.Easy || minDepth > 3 || maxDepth < 5)
        {
            return [(minDepth, maxDepth)];
        }

        var preferredDepth = random.Next(100) switch
        {
            < 60 => 4,
            < 90 => 3,
            _ => 5
        };

        return new[]
            {
                preferredDepth,
                4,
                3,
                5
            }
            .Distinct()
            .Select(depth => (depth, depth))
            .Append((minDepth, maxDepth))
            .ToList();
    }
}
