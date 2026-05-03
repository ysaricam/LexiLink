using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class StepMustBeValidRule : IBusinessRule
{
    private readonly LinkId _nextLinkId;
    private readonly IReadOnlyCollection<LinkId> _validOutgoingLinkIds;

    public StepMustBeValidRule(LinkId nextLinkId, IReadOnlyCollection<LinkId> validOutgoingLinkIds)
    {
        _nextLinkId = nextLinkId;
        _validOutgoingLinkIds = validOutgoingLinkIds;
    }

    public bool IsBroken() => !_validOutgoingLinkIds.Contains(_nextLinkId);

    public string Message => "The selected link is not a valid next step for the current word.";
}
