using LexiLink.Common.Application.Data;
using LexiLink.Modules.Payments.Application.Configuration.Queries;

namespace LexiLink.Modules.Payments.Application.PaymentProducts.GetPaymentProducts;

internal sealed class GetPaymentProductsQueryHandler
    : IQueryHandler<GetPaymentProductsQuery, IReadOnlyList<PaymentProductDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetPaymentProductsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<IReadOnlyList<PaymentProductDto>> Handle(
        GetPaymentProductsQuery request,
        CancellationToken cancellationToken) =>
        PaymentProductSql.GetProductsAsync(
            _sqlConnectionFactory,
            request.Platform,
            activeOnly: true,
            cancellationToken);
}
