using LexiLink.Common.Application.Data;
using LexiLink.Modules.Payments.Application.Configuration.Queries;
using LexiLink.Modules.Payments.Application.PaymentProducts;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.GetAdminPaymentProduct;

internal sealed class GetAdminPaymentProductQueryHandler
    : IQueryHandler<GetAdminPaymentProductQuery, PaymentProductDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetAdminPaymentProductQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<PaymentProductDto> Handle(
        GetAdminPaymentProductQuery request,
        CancellationToken cancellationToken) =>
        PaymentProductSql.GetProductAsync(
            _sqlConnectionFactory,
            request.PaymentProductId,
            cancellationToken);
}
