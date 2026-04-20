using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.GameLinks;

public sealed class LinkId : TypedIdValueBase
{
    public LinkId(Guid value) : base(value)
    {
    }
}