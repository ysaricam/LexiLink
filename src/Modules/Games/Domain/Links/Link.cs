using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Links.Events;
using LexiLink.Modules.Games.Domain.Links.Rules;

namespace LexiLink.Modules.Games.Domain.Links;

public class Link : Entity, IAggregateRoot
{
    public LinkId Id { get; }
    private readonly List<OutgoingLink> _outgoingLinks;
    private readonly CategoryId _categoryId;
    private string _value;
    private string _description;
    private bool _isActive;

    private Link()
    {
        _outgoingLinks = [];
    }

    internal static Link Create(
        CategoryId categoryId,
        string value,
        string description,
        bool isActive)
    {
        return new Link(categoryId, value, description, isActive);
    }

    private Link(
        CategoryId categoryId,
        string value,
        string description,
        bool isActive
    )
    {
        Id = new LinkId(Guid.NewGuid());
        _categoryId = categoryId;
        _value = value;
        _description = description;
        _isActive = isActive;

        _outgoingLinks = [];

        AddDomainEvent(new LinkCreatedDomainEvent(Id));
    }

    public void AddOutgoingLink(LinkId outgoingLinkId)
    {
        CheckRule(new LinkCannotPointToItselfRule(Id, outgoingLinkId));
        CheckRule(new LinkOutgoingAlreadyExistsRule(_outgoingLinks, outgoingLinkId));

        _outgoingLinks.Add(new OutgoingLink(outgoingLinkId));
        AddDomainEvent(new OutgoingLinkAddedDomainEvent(Id, outgoingLinkId));
    }

    public void RemoveOutgoingLink(LinkId outgoingLinkId)
    {
        CheckRule(new LinkOutgoingMustExistRule(_outgoingLinks, outgoingLinkId));

        _outgoingLinks.RemoveAll(o => o.TargetId == outgoingLinkId);
        AddDomainEvent(new OutgoingLinkRemovedDomainEvent(Id, outgoingLinkId));
    }

    public void Activate()
    {
        CheckRule(new LinkMustBeInactiveToActivateRule(_isActive));

        _isActive = true;
        AddDomainEvent(new LinkActivatedDomainEvent(Id));
    }

    public void Deactivate()
    {
        CheckRule(new LinkMustBeActiveToDeactivateRule(_isActive));

        _isActive = false;
        AddDomainEvent(new LinkDeactivatedDomainEvent(Id));
    }
}