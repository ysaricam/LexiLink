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

    public override bool Equals(object? obj)
    {
        if (obj is not Link other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Link? left, Link? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Link? left, Link? right)
    {
        return !(left == right);
    }
}