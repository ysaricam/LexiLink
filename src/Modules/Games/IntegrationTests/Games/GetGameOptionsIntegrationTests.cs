using LexiLink.Modules.Games.Application.Games.GetGameOptions;
using LexiLink.Modules.Games.Application.Games.MakeStep;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.IntegrationTests.Categories;
using LexiLink.Modules.Games.IntegrationTests.Links;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Games.IntegrationTests.Games;

[TestFixture]
public class GetGameOptionsIntegrationTests : TestBase
{
    [Test]
    public async Task GetGameOptions_AtMostSixOutlinks_ReturnsAllAvailable()
    {
        var setup = await GameHelper.SetupChainedGameAsync(Sender);
        await ExecuteCommandAsync(new StartGameCommand(setup.GameId));

        var options = await ExecuteQueryAsync(new GetGameOptionsQuery(setup.GameId));

        // Chain endpoints have 1 outlink; interior have 2. Either way <= 6.
        options.Should().NotBeEmpty();
        options.Count.Should().BeLessThanOrEqualTo(6);
        options.Select(o => o.Id).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task GetGameOptions_StarGraphWithEightOutlinks_ReturnsExactlySix()
    {
        var (gameId, _, _) = await SetupStarGraphGameAsync();

        var options = await ExecuteQueryAsync(new GetGameOptionsQuery(gameId));

        options.Count.Should().Be(6);
        options.Select(o => o.Id).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task GetGameOptions_AfterStep_PreviousLinkIsAlwaysIncluded()
    {
        // Build the star graph and step from one leaf into the center.
        // From the center the player has 8 outlinks; the previous (the leaf
        // we came from) must always survive the selection so the player can
        // backtrack.
        var (gameId, centerId, leafIds) = await SetupStarGraphGameAsync(
            startAt: StarStart.Leaf);

        var firstLeaf = leafIds[0];
        await ExecuteCommandAsync(new MakeStepCommand(gameId, centerId));

        var options = await ExecuteQueryAsync(new GetGameOptionsQuery(gameId));

        options.Count.Should().Be(6);
        options.Should().Contain(o => o.Id == firstLeaf,
            "previous link must always remain in the returned options");
        options[0].Id.Should().Be(firstLeaf,
            "previous link is returned first by convention");
    }

    [Test]
    public async Task GetGameOptions_AfterBacktracking_UsesSimplifiedPathParent()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var sportId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "Spor");
        var athleticsId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "Atletizm");
        var jumpId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "Atlama");
        var targetId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "Hedef");

        await LinkHelper.LinkBidirectionallyAsync(Sender, sportId, athleticsId);
        await LinkHelper.LinkBidirectionallyAsync(Sender, athleticsId, jumpId);
        await LinkHelper.LinkBidirectionallyAsync(Sender, jumpId, targetId);

        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        await DbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "games"."Games" (
                "Id", "PlayerId", "CurrentLinkId", "State", "CategoryId",
                "Difficulty", "StartLinkId", "TargetLinkId",
                "Score", "MaxSteps", "StepsTaken",
                "HintsRemaining", "HintsUsed",
                "UndosUsed",
                "ResetsUsed"
            ) VALUES (
                {0}, {1}, {2}, 'InProgress', {3},
                'Easy', {4}, {5},
                NULL, 20, 0,
                3, 0,
                0,
                0
            );
            """,
            gameId, playerId, sportId, categoryId, sportId, targetId);

        await ExecuteCommandAsync(new MakeStepCommand(gameId, athleticsId));
        await ExecuteCommandAsync(new MakeStepCommand(gameId, jumpId));
        await ExecuteCommandAsync(new MakeStepCommand(gameId, athleticsId));

        var options = await ExecuteQueryAsync(new GetGameOptionsQuery(gameId));

        options[0].Id.Should().Be(sportId,
            "backtracking should pop the path so the next back option is the parent of Atletizm");
        options.Select(o => o.Id).Should().Contain(jumpId,
            "the forward branch should remain available as a normal option");
    }

    [Test]
    public async Task GetGameOptions_IsDeterministic_AcrossRepeatedCalls()
    {
        var (gameId, _, _) = await SetupStarGraphGameAsync();

        var first = await ExecuteQueryAsync(new GetGameOptionsQuery(gameId));
        var second = await ExecuteQueryAsync(new GetGameOptionsQuery(gameId));

        second.Select(o => o.Id).Should().Equal(first.Select(o => o.Id));
    }

    [Test]
    public async Task GetGameOptions_ReachabilityIsolatedLeaf_IsAlwaysIncluded()
    {
        // From `center` the player has 7 outlinks. Six of them form a tight
        // cluster (every pair shares 5 common neighbors) so the density
        // heuristic would normally lock them in and drop the 7th. But the 7th
        // (`isolated`) is the ONLY path toward `target`, so dropping it would
        // soft-brick the puzzle. The handler must resolve the first hop toward
        // target via BFS and lock it in.
        var (gameId, _, isolatedId) = await SetupTargetReachabilityGameAsync();

        var options = await ExecuteQueryAsync(new GetGameOptionsQuery(gameId));

        options.Count.Should().Be(6);
        options.Should().Contain(o => o.Id == isolatedId,
            "the only outlink leading toward target must be locked in");
    }

    /// <summary>
    /// Seeds: 1 Category; `center` connected to 6 cluster leafs (cluster1..6)
    /// AND `isolated`; cluster leafs are fully connected to each other;
    /// `isolated` is the only outlink from `center` that bridges to `target`.
    /// Game starts at center; target is reachable only through `isolated`.
    /// </summary>
    private async Task<(Guid GameId, Guid CenterId, Guid IsolatedId)>
        SetupTargetReachabilityGameAsync()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var centerId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "center");

        var clusterIds = new List<Guid>();
        for (var i = 1; i <= 6; i++)
        {
            var clusterId = await LinkHelper.CreateLinkAsync(
                Sender, categoryId, $"cluster{i}");
            await LinkHelper.LinkBidirectionallyAsync(Sender, centerId, clusterId);
            clusterIds.Add(clusterId);
        }
        for (var i = 0; i < clusterIds.Count; i++)
        {
            for (var j = i + 1; j < clusterIds.Count; j++)
            {
                await LinkHelper.LinkBidirectionallyAsync(
                    Sender, clusterIds[i], clusterIds[j]);
            }
        }

        var isolatedId = await LinkHelper.CreateLinkAsync(
            Sender, categoryId, "isolated");
        await LinkHelper.LinkBidirectionallyAsync(Sender, centerId, isolatedId);

        var targetLinkId = await LinkHelper.CreateLinkAsync(
            Sender, categoryId, "target");
        await LinkHelper.LinkBidirectionallyAsync(Sender, isolatedId, targetLinkId);

        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        await DbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "games"."Games" (
                "Id", "PlayerId", "CurrentLinkId", "State", "CategoryId",
                "Difficulty", "StartLinkId", "TargetLinkId",
                "Score", "MaxSteps", "StepsTaken",
                "HintsRemaining", "HintsUsed",
                "UndosUsed",
                "ResetsUsed"
            ) VALUES (
                {0}, {1}, {2}, 'InProgress', {3},
                'Easy', {4}, {5},
                NULL, 20, 0,
                3, 0,
                0,
                0
            );
            """,
            gameId, playerId, centerId, categoryId, centerId, targetLinkId);

        return (gameId, centerId, isolatedId);
    }

    private enum StarStart { Center, Leaf }

    /// <summary>
    /// Seeds: 1 Category, 1 center link, 8 leaf links each bidirectionally
    /// connected to the center. Then inserts a Game row directly with the
    /// chosen start link so we control the exact game state we want to
    /// exercise the selector on. The first leaf is always wired as the
    /// game's official Start (so MakeStep into the center succeeds).
    /// </summary>
    private async Task<(Guid GameId, Guid CenterId, IReadOnlyList<Guid> LeafIds)>
        SetupStarGraphGameAsync(StarStart startAt = StarStart.Center)
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var centerId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "center");

        var leafIds = new List<Guid>();
        for (var i = 1; i <= 8; i++)
        {
            var leafId = await LinkHelper.CreateLinkAsync(
                Sender, categoryId, $"leaf{i}");
            await LinkHelper.LinkBidirectionallyAsync(Sender, centerId, leafId);
            leafIds.Add(leafId);
        }

        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var startLinkId = startAt switch
        {
            StarStart.Leaf => leafIds[0],
            _ => centerId,
        };
        // Use the last leaf as a placeholder target — it's reachable in 1-2 hops.
        var targetLinkId = leafIds[^1];

        await DbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "games"."Games" (
                "Id", "PlayerId", "CurrentLinkId", "State", "CategoryId",
                "Difficulty", "StartLinkId", "TargetLinkId",
                "Score", "MaxSteps", "StepsTaken",
                "HintsRemaining", "HintsUsed",
                "UndosUsed",
                "ResetsUsed"
            ) VALUES (
                {0}, {1}, {2}, 'InProgress', {3},
                'Easy', {4}, {5},
                NULL, 20, 0,
                3, 0,
                0,
                0
            );
            """,
            gameId, playerId, startLinkId, categoryId, startLinkId, targetLinkId);

        return (gameId, centerId, leafIds);
    }
}
