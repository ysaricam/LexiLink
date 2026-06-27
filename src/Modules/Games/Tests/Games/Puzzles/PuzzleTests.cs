using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;
using LexiLink.Modules.Games.Tests.SeedWork;
using NSubstitute;

namespace LexiLink.Modules.Games.Tests.Games.Puzzles;

[TestFixture]
public class PuzzleTests : TestBase
{
    private const Difficulty AnyDifficulty = Difficulty.Easy;

    private static CategoryId NewCategoryId() => new(Guid.NewGuid());
    private static LinkId NewLinkId() => new(Guid.NewGuid());

    private static IReadOnlyList<LinkId> CategoryLinks(int count)
        => Enumerable.Range(0, count).Select(_ => NewLinkId()).ToList();

    private static IPathFinderService PathFinder(LinkId? target, IReadOnlyList<LinkId> optimalPath)
    {
        var pathFinder = Substitute.For<IPathFinderService>();
        pathFinder
            .FindTarget(Arg.Any<LinkId>(), Arg.Any<IReadOnlyList<LinkId>>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(target);
        if (target is not null)
        {
            pathFinder
                .FindOptimalPath(Arg.Any<LinkId>(), target)
                .Returns(optimalPath);
        }
        else
        {
            pathFinder
                .FindOptimalPath(Arg.Any<LinkId>(), Arg.Any<LinkId>())
                .Returns([]);
        }

        return pathFinder;
    }

    private static IPathFinderService DirectPathFinder()
    {
        var pathFinder = Substitute.For<IPathFinderService>();
        pathFinder
            .FindOptimalPath(Arg.Any<LinkId>(), Arg.Any<LinkId>())
            .Returns(callInfo => new List<LinkId> { callInfo.ArgAt<LinkId>(1) });
        return pathFinder;
    }

    private static IGameConfigurationService GameConfig(int minDepth = 1, int maxDepth = 100)
    {
        var config = Substitute.For<IGameConfigurationService>();
        config.ResolveDepthRange(Arg.Any<Difficulty>()).Returns((minDepth, maxDepth));
        return config;
    }

    private static IPathFinderService PathFinderByTargetDepth(IReadOnlyDictionary<LinkId, int> depthsByTarget)
    {
        var pathFinder = Substitute.For<IPathFinderService>();
        pathFinder
            .FindOptimalPath(Arg.Any<LinkId>(), Arg.Any<LinkId>())
            .Returns(callInfo =>
            {
                var target = callInfo.ArgAt<LinkId>(1);
                return Enumerable
                    .Range(0, depthsByTarget[target])
                    .Select(_ => NewLinkId())
                    .ToList();
            });
        return pathFinder;
    }

    [Test]
    public void Create_WithFewerThanFiveCategoryLinks_BreaksCategoryMustHaveEnoughLinksToStartGameRule()
    {
        var fourLinks = CategoryLinks(4);

        AssertBrokenRule<CategoryMustHaveEnoughLinksToStartGameRule>(
            () => Puzzle.Create(NewCategoryId(), fourLinks, AnyDifficulty,
                PathFinder(NewLinkId(), [NewLinkId()]), GameConfig(), new Random(0)));
    }

    [Test]
    public void Create_WhenPathFinderCannotFindReachableTarget_BreaksPuzzleTargetLinkMustBeReachableRule()
    {
        var fiveLinks = CategoryLinks(5);

        AssertBrokenRule<PuzzleTargetLinkMustBeReachableRule>(
            () => Puzzle.Create(NewCategoryId(), fiveLinks, AnyDifficulty,
                PathFinder(target: null, optimalPath: []), GameConfig(), new Random(0)));
    }

    [Test]
    public void Create_WithValidInput_PopulatesPuzzleFields()
    {
        var categoryId = NewCategoryId();
        var fiveLinks = CategoryLinks(5);
        var target = fiveLinks[0];
        var optimal = new List<LinkId> { NewLinkId(), target };

        var puzzle = Puzzle.Create(categoryId, fiveLinks, Difficulty.Medium,
            PathFinder(target, optimal), GameConfig(), new Random(0));

        puzzle.CategoryId.Should().Be(categoryId);
        puzzle.Difficulty.Should().Be(Difficulty.Medium);
        puzzle.TargetLinkId.Should().Be(target);
        puzzle.OptimalPath.Should().BeEquivalentTo(optimal);
        puzzle.Depth.Should().Be(optimal.Count);
        fiveLinks.Should().Contain(puzzle.StartLinkId);
    }

    [TestCase(0, 4)]
    [TestCase(60, 3)]
    [TestCase(90, 5)]
    public void Create_ForEasyDifficulty_PrefersWeightedTargetDepth(int randomRoll, int expectedDepth)
    {
        var categoryId = NewCategoryId();
        var links = CategoryLinks(6);
        var depthsByTarget = new Dictionary<LinkId, int>
        {
            [links[1]] = 4,
            [links[2]] = 3,
            [links[3]] = 5,
            [links[4]] = 4,
            [links[5]] = 3
        };

        var puzzle = Puzzle.Create(
            categoryId,
            links,
            Difficulty.Easy,
            PathFinderByTargetDepth(depthsByTarget),
            GameConfig(minDepth: 3, maxDepth: 5),
            new FixedFirstRollRandom(randomRoll));

        puzzle.Depth.Should().Be(expectedDepth);
    }

    [Test]
    public void Create_WhenPlayerCompletedPairsExist_DoesNotReuseCompletedStartTargetPair()
    {
        var categoryId = NewCategoryId();
        var links = CategoryLinks(5);
        var allowedPair = new CompletedGameLinkPair(links[0], links[1]);
        var completedPairs = links
            .SelectMany(start => links
                .Where(target => target != start)
                .Select(target => new CompletedGameLinkPair(start, target)))
            .Where(pair => pair != allowedPair)
            .ToList();

        var puzzle = Puzzle.Create(
            categoryId,
            links,
            Difficulty.Easy,
            DirectPathFinder(),
            GameConfig(minDepth: 1, maxDepth: 1),
            new Random(0),
            completedPairs);

        puzzle.StartLinkId.Should().Be(allowedPair.StartLinkId);
        puzzle.TargetLinkId.Should().Be(allowedPair.TargetLinkId);
    }

    [Test]
    public void RequestHint_WhenCurrentLinkIsOnOptimalPath_ReturnsCorrectPathWithNextStep()
    {
        var categoryId = NewCategoryId();
        var n1 = NewLinkId();
        var n2 = NewLinkId();
        var target = NewLinkId();
        var fiveLinks = new List<LinkId> { NewLinkId(), n1, n2, target, NewLinkId() };
        var puzzle = Puzzle.Create(categoryId, fiveLinks, Difficulty.Easy,
            PathFinder(target, [n1, n2, target]), GameConfig(), new Random(0));
        var resolver = Resolver((n1, [n2]), (n2, [target]));

        var hint = puzzle.RequestHint(n1, resolver);

        hint.Type.Should().Be(HintType.OnCorrectPath);
        hint.RecommendedLinkId.Should().Be(n2);
    }

    [Test]
    public void RequestHint_WhenCurrentLinkIsNotOnOptimalPath_ReturnsWrongPathWithFirstHopFromCurrent()
    {
        // Player wandered off path entirely. Live BFS still finds a
        // route from currentLink to target through n1 — the FIRST HOP
        // is what gets returned, not the original optimalPath[0].
        var categoryId = NewCategoryId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var someOtherLink = NewLinkId();
        var fiveLinks = new List<LinkId> { someOtherLink, n1, target, NewLinkId(), NewLinkId() };
        var puzzle = Puzzle.Create(categoryId, fiveLinks, Difficulty.Easy,
            PathFinder(target, [n1, target]), GameConfig(), new Random(0));
        var resolver = Resolver((someOtherLink, [n1]), (n1, [target]));

        var hint = puzzle.RequestHint(someOtherLink, resolver);

        hint.Type.Should().Be(HintType.OnWrongPath);
        hint.RecommendedLinkId.Should().Be(n1);
    }

    [Test]
    public void RequestHint_WhenCurrentIsTargetLink_ReturnsWrongPathWithSelf()
    {
        // Standing on target means there is no "next" step to recommend.
        // The hint returns WrongPath with current (target) as the
        // recommendation; the UI never asks for hint after Complete,
        // but this keeps the contract total.
        var categoryId = NewCategoryId();
        var n1 = NewLinkId();
        var target = NewLinkId();
        var fiveLinks = new List<LinkId> { NewLinkId(), n1, target, NewLinkId(), NewLinkId() };
        var puzzle = Puzzle.Create(categoryId, fiveLinks, Difficulty.Easy,
            PathFinder(target, [n1, target]), GameConfig(), new Random(0));
        var resolver = Resolver();

        var hint = puzzle.RequestHint(target, resolver);

        hint.Type.Should().Be(HintType.OnWrongPath);
        hint.RecommendedLinkId.Should().Be(target);
    }

    private static ILinkNeighborResolver Resolver(
        params (LinkId From, IReadOnlyCollection<LinkId> To)[] mappings)
    {
        var resolver = Substitute.For<ILinkNeighborResolver>();
        resolver.GetOutgoingLinkIds(Arg.Any<LinkId>()).Returns(Array.Empty<LinkId>());
        foreach (var (from, to) in mappings)
        {
            resolver.GetOutgoingLinkIds(from).Returns(to);
        }
        return resolver;
    }

    private sealed class FixedFirstRollRandom : Random
    {
        private readonly int _firstRoll;
        private bool _usedFirstRoll;

        public FixedFirstRollRandom(int firstRoll)
        {
            _firstRoll = firstRoll;
        }

        public override int Next(int maxValue)
        {
            if (!_usedFirstRoll)
            {
                _usedFirstRoll = true;
                return _firstRoll;
            }

            return 0;
        }

        public override int Next() => 0;
    }
}
