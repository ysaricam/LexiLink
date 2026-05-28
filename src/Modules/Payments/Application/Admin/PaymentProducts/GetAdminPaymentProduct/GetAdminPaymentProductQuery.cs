using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Application.PaymentProducts;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.GetAdminPaymentProduct;

public sealed class GetAdminPaymentProductQuery : QueryBase<PaymentProductDto>
{
    public Guid PaymentProductId { get; }

    public GetAdminPaymentProductQuery(Guid paymentProductId)
    {
        PaymentProductId = paymentProductId;
    }
}
