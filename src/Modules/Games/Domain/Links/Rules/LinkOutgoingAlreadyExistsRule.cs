using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Rules;

public class LinkOutgoingAlreadyExistsRule : IBusinessRule
{
    private readonly IReadOnlyCollection<OutgoingLink> _outgoingLinks;
    private readonly LinkId _newOutgoingLinkId;

    public LinkOutgoingAlreadyExistsRule(IReadOnlyCollection<OutgoingLink> outgoingLinks, LinkId newOutgoingLinkId)
    {
        _outgoingLinks = outgoingLinks;
        _newOutgoingLinkId = newOutgoingLinkId;
    }

    public bool IsBroken() => _outgoingLinks.Any(o => o.TargetId == _newOutgoingLinkId);

    public string Message => "Outgoing link already exists.";
}
