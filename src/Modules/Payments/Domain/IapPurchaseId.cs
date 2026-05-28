using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain;

public sealed class IapPurchaseId : TypedIdValueBase
{
    public IapPurchaseId(Guid value) : base(value)
    {
    }
}
