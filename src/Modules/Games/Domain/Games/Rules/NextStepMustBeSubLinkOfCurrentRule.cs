using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class NextStepMustBeSubLinkOfCurrentRule : IBusinessRule
{
    private readonly LinkId _nextLinkId;
    private readonly IReadOnlyList<LinkId> _availableSubLinkIds;

    public NextStepMustBeSubLinkOfCurrentRule(LinkId nextLinkId, IReadOnlyList<LinkId> availableSubLinkIds)
    {
        _nextLinkId = nextLinkId;
        _availableSubLinkIds = availableSubLinkIds;
    }

    public bool IsBroken() => _nextLinkId is null || !_availableSubLinkIds.Any(id => id.Equals(_nextLinkId));

    public string Message => _nextLinkId is null 
        ? "Sonraki bağlantı boş olamaz." 
        : $"Geçersiz hamle: Bu kelime mevcut kelimeden sonra seçilemez.";
}
