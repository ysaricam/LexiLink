using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links;

public sealed class OutgoingLink : ValueObject
{
    public LinkId TargetId { get; } = default!;

    private OutgoingLink() { }

    internal OutgoingLink(LinkId targetId)
    {
        TargetId = targetId;
    }
}
