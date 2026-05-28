using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.PaymentProducts;

public sealed record PaymentProductDto(
    Guid Id,
    string StoreProductId,
    int DiamondAmount,
    bool IsAppleAvailable,
    bool IsGoogleAvailable,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<PaymentPlatform> Platforms);
