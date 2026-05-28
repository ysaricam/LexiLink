using LexiLink.Common.Application.Data;
using LexiLink.Modules.Payments.Application.Configuration.Queries;

namespace LexiLink.Modules.Payments.Application.Admin.IapPurchases.GetAdminIapPurchases;

internal sealed class GetAdminIapPurchasesQueryHandler
    : IQueryHandler<GetAdminIapPurchasesQuery, IReadOnlyList<AdminIapPurchaseDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetAdminIapPurchasesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<IReadOnlyList<AdminIapPurchaseDto>> Handle(
        GetAdminIapPurchasesQuery request,
        CancellationToken cancellationToken) =>
        AdminIapPurchaseSql.GetPurchasesAsync(
            _sqlConnectionFactory,
            request.PlayerId,
            request.Platform,
            request.Status,
            request.StoreProductId,
            request.Limit,
            request.Offset,
            cancellationToken);
}
