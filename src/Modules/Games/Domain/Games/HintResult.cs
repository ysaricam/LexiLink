using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games;

public enum HintStatus
{
    OnCorrectPath,
    RedirectedToSafety
}

public sealed class HintResult : ValueObject
{
    public string Message { get; }
    public LinkId RecommendedLinkId { get; }
    public HintStatus Status { get; }

    public HintResult(string message, LinkId recommendedLinkId, HintStatus status)
    {
        // Mesaj boş olamaz gibi basit bir kural eklenebilir
        Message = message;
        RecommendedLinkId = recommendedLinkId;
        Status = status;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Message;
        yield return RecommendedLinkId;
        yield return Status;
    }
}