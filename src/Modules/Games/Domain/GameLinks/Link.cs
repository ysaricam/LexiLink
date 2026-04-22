using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks.Rules;
using LexiLink.Modules.Games.Domain.GameLinks.Events;

namespace LexiLink.Modules.Games.Domain.GameLinks;

public sealed class Link : AggregateRoot
{
    public LinkId Id { get; private set; }
    public string Value { get; private set; }
    
    private readonly List<LinkId> _subLinkIds;
    public IReadOnlyList<LinkId> SubLinkIds => _subLinkIds;

    internal static Link CreateNew(string value, IEnumerable<LinkId>? subLinkIds = null)
    {
        return new Link(value, subLinkIds);
    }

    private Link(string value, IEnumerable<LinkId>? subLinkIds = null)
    {
        CheckRule(new LinkCannotBeEmptyRule(value));

        Id = new LinkId(Guid.NewGuid());
        Value = value.Trim();
        _subLinkIds = (subLinkIds ?? Enumerable.Empty<LinkId>()).ToList();

        this.AddDomainEvent(new LinkCreatedDomainEvent(this.Id, this.Value));
    }
}