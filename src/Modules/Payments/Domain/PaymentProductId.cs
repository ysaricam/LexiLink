using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain;

public sealed class PaymentProductId : TypedIdValueBase
{
    public PaymentProductId(Guid value) : base(value)
    {
    }
}
