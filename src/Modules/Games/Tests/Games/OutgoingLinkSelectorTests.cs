using LexiLink.Modules.Games.Application.Games.GetGameOptions;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class OutgoingLinkSelectorTests
{
    private static readonly Guid A = Id(1);
    private static readonly Guid B = Id(2);
    private static readonly Guid C = Id(3);
    private static readonly Guid D = Id(4);
    private static readonly Guid E = Id(5);
    private static readonly Guid F = Id(6);
    private static readonly Guid G = Id(7);
    private static readonly Guid H = Id(8);

    [Test]
    public void Select_WhenCandidateCountAtMostLimit_ReturnsAllSortedById()
    {
        var candidates = new[] { D, A, C, B };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: null,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().Equal(A, B, C, D);
    }

    [Test]
    public void Select_WhenAtMostLimitAndPreviousProvided_PutsPreviousFirst()
    {
        var candidates = new[] { D, A, C, B };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: C,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().Equal(C, A, B, D);
    }

    [Test]
    public void Select_PreviousLinkIdAlwaysLockedIntoResult()
    {
        // 7 candidates; previous is the link with the worst possible score
        // (no pairwise commonality with anyone). The selector must still keep it.
        var candidates = new[] { A, B, C, D, E, F, G };
        var pairwise = new Dictionary<(Guid Left, Guid Right), int>
        {
            [(B, C)] = 5,
            [(B, D)] = 4,
            [(C, D)] = 4,
            [(D, E)] = 3,
            [(E, F)] = 2,
        };
        var degrees = candidates.ToDictionary(id => id, _ => 0);

        var result = OutgoingLinkSelector.Select(
            candidates, degrees, pairwise,
            previousLinkId: A,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().Contain(A);
        result[0].Should().Be(A, "previous must be returned first");
        result.Count.Should().Be(6);
    }

    [Test]
    public void Select_DeterministicTieBreak_LowerIdWins()
    {
        // All pairwise scores zero; all degrees equal — must fall back to id ASC.
        var candidates = new[] { F, A, D, B, E, C, G };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: null,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().Equal(A, B, C, D, E, F);
    }

    [Test]
    public void Select_DegreeFallbackWhenAllPairwiseZero()
    {
        // Pairwise empty → tie-break uses degree DESC (then id ASC).
        var candidates = new[] { A, B, C, D, E, F, G };
        var degrees = new Dictionary<Guid, int>
        {
            [A] = 1, [B] = 1, [C] = 9, [D] = 5, [E] = 3, [F] = 3, [G] = 7,
        };

        var result = OutgoingLinkSelector.Select(
            candidates, degrees, EmptyPairwise(),
            previousLinkId: null,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().Equal(C, G, D, E, F, A);
    }

    [Test]
    public void Select_PreviousLinkIdNotInCandidates_IsIgnored()
    {
        var candidates = new[] { A, B, C, D, E, F, G };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: H,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().Equal(A, B, C, D, E, F);
        result.Should().NotContain(H);
    }

    [Test]
    public void Select_SeedsByHighestPairwiseScore_WhenNoPrevious()
    {
        // Build a graph where {B, C, D, E, F, G} form a tightly connected
        // cluster and A is an isolated candidate. With 7 candidates and no
        // previous, the greedy must drop A and pick the cluster.
        var candidates = new[] { A, B, C, D, E, F, G };
        var cluster = new[] { B, C, D, E, F, G };
        var pairwise = new Dictionary<(Guid Left, Guid Right), int>();
        foreach (var (x, y) in PairsOf(cluster))
        {
            var key = x.CompareTo(y) < 0 ? (x, y) : (y, x);
            pairwise[key] = 5;
        }

        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            pairwise,
            previousLinkId: null,
            pathToTargetLinkId: null,
            limit: 6);

        result.Should().BeEquivalentTo(cluster);
        result.Should().NotContain(A);
    }

    [Test]
    public void Select_RespectsLimit()
    {
        var candidates = new[] { A, B, C, D, E, F, G, H };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: null,
            pathToTargetLinkId: null,
            limit: 6);

        result.Count.Should().Be(6);
    }

    [Test]
    public void Select_ProducesSameResultOnRepeatedCalls()
    {
        var candidates = new[] { A, B, C, D, E, F, G, H };
        var pairwise = new Dictionary<(Guid Left, Guid Right), int>
        {
            [(A, B)] = 1,
            [(B, C)] = 1,
            [(C, D)] = 1,
            [(D, E)] = 1,
            [(E, F)] = 1,
            [(F, G)] = 1,
            [(G, H)] = 1,
        };
        var degrees = candidates.ToDictionary(id => id, _ => 2);

        var first = OutgoingLinkSelector.Select(
            candidates, degrees, pairwise,
            previousLinkId: A, pathToTargetLinkId: null, limit: 6);
        var second = OutgoingLinkSelector.Select(
            candidates, degrees, pairwise,
            previousLinkId: A, pathToTargetLinkId: null, limit: 6);

        second.Should().Equal(first);
    }

    [Test]
    public void Select_LocksPathToTargetLinkId_EvenWhenIsolated()
    {
        // Same scaffolding as Select_SeedsByHighestPairwiseScore_WhenNoPrevious:
        // A is fully isolated, {B..G} form a tight cluster (all pairs score 5).
        // But here A is the only outlink that leads toward the target, so the
        // selector must lock A even though density wants to drop it.
        var candidates = new[] { A, B, C, D, E, F, G };
        var cluster = new[] { B, C, D, E, F, G };
        var pairwise = new Dictionary<(Guid Left, Guid Right), int>();
        foreach (var (x, y) in PairsOf(cluster))
        {
            var key = x.CompareTo(y) < 0 ? (x, y) : (y, x);
            pairwise[key] = 5;
        }

        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            pairwise,
            previousLinkId: null,
            pathToTargetLinkId: A,
            limit: 6);

        result.Should().Contain(A, "path-to-target must survive density-based pruning");
        result.Count.Should().Be(6);
    }

    [Test]
    public void Select_LocksBothPreviousAndPathToTarget()
    {
        // 8 candidates, both locks active, 4 greedy slots remain.
        var candidates = new[] { A, B, C, D, E, F, G, H };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: H,
            pathToTargetLinkId: G,
            limit: 6);

        result.Count.Should().Be(6);
        result[0].Should().Be(H, "previous is the first slot");
        result[1].Should().Be(G, "path-to-target is the second slot");
        result.Distinct().Count().Should().Be(6, "no duplicates");
    }

    [Test]
    public void Select_PreviousAndPathToTargetSameLink_LocksOnce()
    {
        // Edge case: starting move where backtracking == target direction.
        var candidates = new[] { A, B, C, D, E, F, G, H };
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: A,
            pathToTargetLinkId: A,
            limit: 6);

        result.Count.Should().Be(6);
        result[0].Should().Be(A);
        result.Distinct().Count().Should().Be(6, "no duplicates when locks collide");
    }

    [Test]
    public void Select_PathToTargetIdNotInCandidates_IsIgnored()
    {
        // Defensive: handler resolved a hop that's not in candidates (race or
        // inactive link). Selector must not crash and must still return 6.
        var candidates = new[] { A, B, C, D, E, F, G };
        var unknown = Id(99);
        var result = OutgoingLinkSelector.Select(
            candidates,
            EmptyDegrees(candidates),
            EmptyPairwise(),
            previousLinkId: null,
            pathToTargetLinkId: unknown,
            limit: 6);

        result.Should().NotContain(unknown);
        result.Count.Should().Be(6);
    }

    private static Guid Id(int n) =>
        new($"00000000-0000-0000-0000-{n:D12}");

    private static Dictionary<Guid, int> EmptyDegrees(IEnumerable<Guid> candidates) =>
        candidates.ToDictionary(id => id, _ => 0);

    private static Dictionary<(Guid Left, Guid Right), int> EmptyPairwise() => new();

    private static IEnumerable<(Guid, Guid)> PairsOf(IReadOnlyList<Guid> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                yield return (items[i], items[j]);
            }
        }
    }
}
