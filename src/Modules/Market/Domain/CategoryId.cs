using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain;

public sealed class CategoryId : TypedIdValueBase
{
    public CategoryId(Guid value) : base(value)
    {
    }
}
