using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Payments.Application.Contracts;

namespace LexiLink.Modules.Payments.Application.Admin.PaymentProducts.UpdatePaymentProduct;

public sealed class UpdatePaymentProductCommand : CommandBase, IAdminCommand
{
    public Guid PaymentProductId { get; }
    public int DiamondAmount { get; }
    public bool IsAppleAvailable { get; }
    public bool IsGoogleAvailable { get; }
    public int SortOrder { get; }

    public UpdatePaymentProductCommand(
        Guid id,
        int diamondAmount,
        bool isAppleAvailable,
        bool isGoogleAvailable,
        int sortOrder)
    {
        PaymentProductId = id;
        DiamondAmount = diamondAmount;
        IsAppleAvailable = isAppleAvailable;
        IsGoogleAvailable = isGoogleAvailable;
        SortOrder = sortOrder;
    }

    public string AuditTargetType => "Payments.PaymentProduct";
    public string? AuditTargetId => PaymentProductId.ToString();
}
