using LexiLink.Modules.Payments.Application.Contracts;

namespace LexiLink.Modules.Payments.Application.Admin.IapPurchases.GetAdminIapPurchase;

public sealed class GetAdminIapPurchaseQuery : QueryBase<AdminIapPurchaseDto>
{
    public Guid IapPurchaseId { get; }

    public GetAdminIapPurchaseQuery(Guid iapPurchaseId)
    {
        IapPurchaseId = iapPurchaseId;
    }
}
