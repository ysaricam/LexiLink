using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Puzzles;

public enum HintType
{
    OnCorrectPath,
    OnWrongPath
}

public sealed class HintResult : ValueObject
{
    public HintType Type { get; }
    public LinkId RecommendedLinkId { get; }

    private HintResult(HintType type, LinkId recommendedLinkId)
    {
        Type = type;
        RecommendedLinkId = recommendedLinkId;
    }

    internal static HintResult CorrectPath(LinkId nextLinkId)
        => new(HintType.OnCorrectPath, nextLinkId);

    internal static HintResult WrongPath(LinkId closestCorrectLinkId)
        => new(HintType.OnWrongPath, closestCorrectLinkId);
}
