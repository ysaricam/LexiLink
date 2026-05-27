using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain;

public sealed class ShopItemId : TypedIdValueBase
{
    public ShopItemId(Guid value) : base(value)
    {
    }
}
