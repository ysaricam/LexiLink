using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Events;

public class LinkActivatedDomainEvent : DomainEvent
{
    public LinkId LinkId { get; }

    public LinkActivatedDomainEvent(LinkId linkId)
    {
        LinkId = linkId;
    }
}
