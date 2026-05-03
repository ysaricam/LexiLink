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

    public IReadOnlyList<LinkId> OptimalPath => _optimalPath.Select(s => s.LinkId).ToList().AsReadOnly();
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
        Random random)
    {
        CheckRule(new CategoryMustHaveEnoughLinksToStartGameRule(categoryLinkIds));

        var startLinkId = ChooseStartLink(categoryLinkIds, random);

        var (minDepth, maxDepth) = gameConfiguration.ResolveDepthRange(difficulty);
        var targetLinkId = ChooseTargetLink(startLinkId, categoryLinkIds, minDepth, maxDepth, pathFinder);

        var optimalPath = pathFinder.FindOptimalPath(startLinkId, targetLinkId);

        return new Puzzle(categoryId, difficulty, startLinkId, targetLinkId, optimalPath);
    }

    public HintResult RequestHint(LinkId currentLinkId)
    {
        var currentIndex = _optimalPath.FindIndex(s => s.LinkId == currentLinkId);
        if (currentIndex >= 0 && currentIndex < _optimalPath.Count - 1)
        {
            return HintResult.CorrectPath(_optimalPath[currentIndex + 1].LinkId);
        }

        var closestCorrectLinkId = _optimalPath.Count > 0 ? _optimalPath[0].LinkId : StartLinkId;
        return HintResult.WrongPath(closestCorrectLinkId);
    }

    private static LinkId ChooseStartLink(IReadOnlyList<LinkId> categoryLinkIds, Random random)
    {
        return categoryLinkIds[random.Next(categoryLinkIds.Count)];
    }

    private static LinkId ChooseTargetLink(
        LinkId startLinkId,
        IReadOnlyList<LinkId> categoryLinkIds,
        int minDepth,
        int maxDepth,
        IPathFinderService pathFinder)
    {
        var targetLinkId = pathFinder.FindTarget(startLinkId, categoryLinkIds, minDepth, maxDepth);
        CheckRule(new PuzzleTargetLinkMustBeReachableRule(targetLinkId));
        return targetLinkId!;
    }
}
