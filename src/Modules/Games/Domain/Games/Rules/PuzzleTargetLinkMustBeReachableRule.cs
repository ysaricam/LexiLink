using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class PuzzleTargetLinkMustBeReachableRule : IBusinessRule
{
    private readonly LinkId? _targetLinkId;

    public PuzzleTargetLinkMustBeReachableRule(LinkId? targetLinkId)
    {
        _targetLinkId = targetLinkId;
    }

    public bool IsBroken() => _targetLinkId == null;

    public string Message => "A reachable target link could not be found for the selected difficulty.";
}
