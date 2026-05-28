using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.PaymentProducts.GetPaymentProducts;

public sealed class GetPaymentProductsQuery : QueryBase<IReadOnlyList<PaymentProductDto>>
{
    public PaymentPlatform Platform { get; }

    public GetPaymentProductsQuery(PaymentPlatform platform)
    {
        Platform = platform;
    }
}
