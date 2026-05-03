using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Events;

public class OutgoingLinkAddedDomainEvent : DomainEvent
{
    public LinkId LinkId { get; }
    public LinkId OutgoingLinkId { get; }

    public OutgoingLinkAddedDomainEvent(LinkId linkId, LinkId outgoingLinkId)
    {
        LinkId = linkId;
        OutgoingLinkId = outgoingLinkId;
    }
}
