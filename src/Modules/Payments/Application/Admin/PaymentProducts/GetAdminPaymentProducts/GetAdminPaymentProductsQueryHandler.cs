using LexiLink.Common.Application.Data;
using LexiLink.Modules.Payments.Application.Configuration.Queries;
using LexiLink.Modules.Payments.Application.PaymentProducts;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.GetAdminPaymentProducts;

internal sealed class GetAdminPaymentProductsQueryHandler
    : IQueryHandler<GetAdminPaymentProductsQuery, IReadOnlyList<PaymentProductDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetAdminPaymentProductsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<PaymentProductDto>> Handle(
        GetAdminPaymentProductsQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await PaymentProductSql.GetProductsAsync(
            _sqlConnectionFactory,
            request.Platform,
            activeOnly: false,
            cancellationToken);

        return request.IsActive is null
            ? rows
            : rows.Where(x => x.IsActive == request.IsActive.Value).ToList();
    }
}
