using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Rules;

public class LinkOutgoingMustExistRule : IBusinessRule
{
    private readonly IReadOnlyCollection<OutgoingLink> _outgoingLinks;
    private readonly LinkId _linkIdToRemove;

    public LinkOutgoingMustExistRule(IReadOnlyCollection<OutgoingLink> outgoingLinks, LinkId linkIdToRemove)
    {
        _outgoingLinks = outgoingLinks;
        _linkIdToRemove = linkIdToRemove;
    }

    public bool IsBroken() => !_outgoingLinks.Any(o => o.TargetId == _linkIdToRemove);

    public string Message => "Outgoing link must exist.";
}
