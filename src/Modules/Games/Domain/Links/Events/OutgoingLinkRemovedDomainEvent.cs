using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Events;

public class OutgoingLinkRemovedDomainEvent : DomainEvent
{
    public LinkId LinkId { get; }
    public LinkId OutgoingLinkId { get; }

    public OutgoingLinkRemovedDomainEvent(LinkId linkId, LinkId outgoingLinkId)
    {
        LinkId = linkId;
        OutgoingLinkId = outgoingLinkId;
    }
}
