using LexiLink.Common.Application.Data;
using LexiLink.Modules.Payments.Application.Configuration.Queries;

namespace LexiLink.Modules.Payments.Application.Admin.IapPurchases.GetAdminIapPurchase;

internal sealed class GetAdminIapPurchaseQueryHandler
    : IQueryHandler<GetAdminIapPurchaseQuery, AdminIapPurchaseDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal GetAdminIapPurchaseQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task<AdminIapPurchaseDto> Handle(
        GetAdminIapPurchaseQuery request,
        CancellationToken cancellationToken) =>
        AdminIapPurchaseSql.GetPurchaseAsync(
            _sqlConnectionFactory,
            request.IapPurchaseId,
            cancellationToken);
}
