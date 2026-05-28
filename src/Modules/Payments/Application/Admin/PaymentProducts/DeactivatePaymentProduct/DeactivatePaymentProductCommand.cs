using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Payments.Application.Contracts;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.DeactivatePaymentProduct;

public sealed class DeactivatePaymentProductCommand : CommandBase, IAdminCommand
{
    public Guid PaymentProductId { get; }

    public DeactivatePaymentProductCommand(Guid paymentProductId)
    {
        PaymentProductId = paymentProductId;
    }

    public string AuditTargetType => "Payments.PaymentProduct";
    public string? AuditTargetId => PaymentProductId.ToString();
}
