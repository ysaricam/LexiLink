namespace LexiLink.Modules.Games.Application.Games.GetGameOptions;

/// <summary>
/// Deterministic densest-k-subgraph selector for game outgoing link options.
/// Locks <paramref name="previousLinkId"/> so the player can always backtrack
/// and <paramref name="pathToTargetLinkId"/> so target reachability is never
/// silently dropped by the density heuristic. Tie-break: pairwise score DESC
/// → degree DESC → id ASC.
/// </summary>
internal static class OutgoingLinkSelector
{
    public static List<Guid> Select(
        IReadOnlyList<Guid> candidates,
        IReadOnlyDictionary<Guid, int> degrees,
        IReadOnlyDictionary<(Guid Left, Guid Right), int> pairwiseCommon,
        Guid? previousLinkId,
        Guid? pathToTargetLinkId,
        int limit)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(degrees);
        ArgumentNullException.ThrowIfNull(pairwiseCommon);
        if (limit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        if (candidates.Count <= limit)
        {
            return Order(candidates, previousLinkId);
        }

        var remaining = new HashSet<Guid>(candidates);
        var selected = new List<Guid>(limit);

        if (previousLinkId is { } prev && remaining.Contains(prev))
        {
            selected.Add(prev);
            remaining.Remove(prev);
        }

        if (pathToTargetLinkId is { } target
            && target != previousLinkId
            && remaining.Contains(target))
        {
            selected.Add(target);
            remaining.Remove(target);
        }

        if (selected.Count == 0)
        {
            var seed = PickBest(
                remaining,
                candidate => SumPairwise(candidate, remaining, pairwiseCommon, exclude: candidate),
                degrees);
            selected.Add(seed);
            remaining.Remove(seed);
        }

        while (selected.Count < limit && remaining.Count > 0)
        {
            var next = PickBest(
                remaining,
                candidate => SumPairwiseAgainstSet(candidate, selected, pairwiseCommon),
                degrees);
            selected.Add(next);
            remaining.Remove(next);
        }

        return Order(selected, previousLinkId);
    }

    private static List<Guid> Order(
        IReadOnlyList<Guid> candidates,
        Guid? previousLinkId)
        => OutgoingLinkOrderer.OrderForDisplay(candidates, previousLinkId);

    private static Guid PickBest(
        HashSet<Guid> pool,
        Func<Guid, int> score,
        IReadOnlyDictionary<Guid, int> degrees) =>
        pool
            .OrderByDescending(score)
            .ThenByDescending(id => degrees.TryGetValue(id, out var d) ? d : 0)
            .ThenBy(id => id)
            .First();

    private static int SumPairwiseAgainstSet(
        Guid candidate,
        IReadOnlyList<Guid> set,
        IReadOnlyDictionary<(Guid Left, Guid Right), int> pairwiseCommon)
    {
        var sum = 0;
        foreach (var other in set)
        {
            sum += PairwiseScore(candidate, other, pairwiseCommon);
        }
        return sum;
    }

    private static int SumPairwise(
        Guid candidate,
        IEnumerable<Guid> pool,
        IReadOnlyDictionary<(Guid Left, Guid Right), int> pairwiseCommon,
        Guid exclude)
    {
        var sum = 0;
        foreach (var other in pool)
        {
            if (other == exclude)
            {
                continue;
            }
            sum += PairwiseScore(candidate, other, pairwiseCommon);
        }
        return sum;
    }

    private static int PairwiseScore(
        Guid a,
        Guid b,
        IReadOnlyDictionary<(Guid Left, Guid Right), int> pairwiseCommon)
    {
        if (a == b)
        {
            return 0;
        }
        var key = a.CompareTo(b) < 0 ? (a, b) : (b, a);
        return pairwiseCommon.TryGetValue(key, out var score) ? score : 0;
    }
}
