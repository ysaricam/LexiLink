using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Events;

public class LinkDeactivatedDomainEvent : DomainEvent
{
    public LinkId LinkId { get; }

    public LinkDeactivatedDomainEvent(LinkId linkId)
    {
        LinkId = linkId;
    }
}
