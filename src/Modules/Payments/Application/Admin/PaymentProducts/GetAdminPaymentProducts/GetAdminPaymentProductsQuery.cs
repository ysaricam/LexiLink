using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Application.PaymentProducts;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.GetAdminPaymentProducts;

public sealed class GetAdminPaymentProductsQuery : QueryBase<IReadOnlyList<PaymentProductDto>>
{
    public PaymentPlatform? Platform { get; }
    public bool? IsActive { get; }

    public GetAdminPaymentProductsQuery(PaymentPlatform? platform, bool? isActive)
    {
        Platform = platform;
        IsActive = isActive;
    }
}
