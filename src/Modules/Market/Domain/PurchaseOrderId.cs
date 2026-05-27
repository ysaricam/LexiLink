using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain;

public sealed class PurchaseOrderId : TypedIdValueBase
{
    public PurchaseOrderId(Guid value) : base(value)
    {
    }
}
