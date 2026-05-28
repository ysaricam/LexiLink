using LexiLink.Common.Domain;

namespace LexiLink.Modules.Payments.Domain;

public sealed class PaymentNotificationId : TypedIdValueBase
{
    public PaymentNotificationId(Guid value) : base(value)
    {
    }
}
