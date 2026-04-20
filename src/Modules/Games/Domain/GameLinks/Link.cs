using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks.Rules;

namespace LexiLink.Modules.Games.Domain.GameLinks;

public sealed class Link : Entity, IAggregateRoot
{
    public LinkId Id { get; private set; }
    public string Value { get; private set; }
    
    private readonly List<LinkId> _subLinkIds;
    public IReadOnlyList<LinkId> SubLinkIds => _subLinkIds;

    public static Link Of(string value, IEnumerable<LinkId>? subLinkIds = null)
    {
        return new Link(value, subLinkIds);
    }

    private Link(string value, IEnumerable<LinkId>? subLinkIds = null)
    {
        CheckRule(new LinkCannotBeEmptyRule(value));

        Id = new LinkId(Guid.NewGuid());
        Value = value.Trim();
        _subLinkIds = (subLinkIds ?? Enumerable.Empty<LinkId>()).ToList();
    }
}