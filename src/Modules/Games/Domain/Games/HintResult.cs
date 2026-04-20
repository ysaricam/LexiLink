using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games;

public enum HintStatus
{
    OnCorrectPath,
    RedirectedToSafety
}

/// <summary>
/// İpucu işleminin sonucunu temsil eden bilgi paketi.
/// </summary>
public sealed record HintResult(
    string Message, 
    LinkId RecommendedLinkId, 
    HintStatus Status);
