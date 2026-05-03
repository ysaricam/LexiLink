using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Rules;

public class LinkCannotPointToItselfRule : IBusinessRule
{
    private readonly LinkId _linkId;
    private readonly LinkId _outgoingLinkId;

    public LinkCannotPointToItselfRule(LinkId linkId, LinkId outgoingLinkId)
    {
        _linkId = linkId;
        _outgoingLinkId = outgoingLinkId;
    }

    public bool IsBroken() => _linkId == _outgoingLinkId;

    public string Message => "Link cannot point to itself.";
}
