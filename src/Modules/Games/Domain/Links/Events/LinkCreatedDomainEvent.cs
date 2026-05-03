using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Events;

public class LinkCreatedDomainEvent : DomainEvent
{
    public LinkId LinkId { get; }

    public LinkCreatedDomainEvent(LinkId linkId)
    {
        LinkId = linkId;
    }
}
