using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.GameLinks.Events;

public class LinkCreatedDomainEvent : DomainEventBase
{
    public LinkId LinkId { get; }
    public string Value { get; }

    public LinkCreatedDomainEvent(LinkId linkId, string value)
    {
        LinkId = linkId;
        Value = value;
    }
}