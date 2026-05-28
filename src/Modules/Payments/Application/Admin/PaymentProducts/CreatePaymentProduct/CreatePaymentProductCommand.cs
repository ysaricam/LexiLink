using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Payments.Application.Contracts;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.CreatePaymentProduct;

public sealed class CreatePaymentProductCommand : CommandBase<Guid>, IAdminCommand
{
    public string StoreProductId { get; }
    public int DiamondAmount { get; }
    public bool IsAppleAvailable { get; }
    public bool IsGoogleAvailable { get; }
    public int SortOrder { get; }

    public CreatePaymentProductCommand(
        string storeProductId,
        int diamondAmount,
        bool isAppleAvailable,
        bool isGoogleAvailable,
        int sortOrder)
    {
        StoreProductId = storeProductId;
        DiamondAmount = diamondAmount;
        IsAppleAvailable = isAppleAvailable;
        IsGoogleAvailable = isGoogleAvailable;
        SortOrder = sortOrder;
    }

    public string AuditTargetType => "Payments.PaymentProduct";
    public string? AuditTargetId => null;
}
