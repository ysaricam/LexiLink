using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.Admin.IapPurchases.GetAdminIapPurchases;

public sealed class GetAdminIapPurchasesQuery : QueryBase<IReadOnlyList<AdminIapPurchaseDto>>
{
    public Guid? PlayerId { get; }
    public PaymentPlatform? Platform { get; }
    public IapPurchaseStatus? Status { get; }
    public string? StoreProductId { get; }
    public int Limit { get; }
    public int Offset { get; }

    public GetAdminIapPurchasesQuery(
        Guid? playerId,
        PaymentPlatform? platform,
        IapPurchaseStatus? status,
        string? storeProductId,
        int limit,
        int offset)
    {
        PlayerId = playerId;
        Platform = platform;
        Status = status;
        StoreProductId = storeProductId;
        Limit = limit;
        Offset = offset;
    }
}
