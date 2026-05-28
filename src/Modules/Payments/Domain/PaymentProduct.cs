using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Domain.Events;
using LexiLink.Modules.Payments.Domain.Rules;

namespace LexiLink.Modules.Payments.Domain;

public class PaymentProduct : Entity, IAggregateRoot
{
    public PaymentProductId Id { get; private set; }
    private StoreProductId _storeProductId = null!;
    private int _diamondAmount;
    private bool _isAppleAvailable;
    private bool _isGoogleAvailable;
    private int _sortOrder;
    private bool _isActive;

    public StoreProductId StoreProductId => _storeProductId;
    public int DiamondAmount => _diamondAmount;
    public bool IsAppleAvailable => _isAppleAvailable;
    public bool IsGoogleAvailable => _isGoogleAvailable;
    public int SortOrder => _sortOrder;
    public bool IsActive => _isActive;

    private PaymentProduct()
    {
        Id = null!;
    }

    private PaymentProduct(
        PaymentProductId id,
        StoreProductId storeProductId,
        int diamondAmount,
        bool isAppleAvailable,
        bool isGoogleAvailable,
        int sortOrder)
    {
        Id = id;
        _storeProductId = storeProductId;
        _diamondAmount = diamondAmount;
        _isAppleAvailable = isAppleAvailable;
        _isGoogleAvailable = isGoogleAvailable;
        _sortOrder = sortOrder;
        _isActive = true;

        AddDomainEvent(new PaymentProductCreatedDomainEvent(Id.Value, _storeProductId.Value));
    }

    internal static PaymentProduct Create(
        StoreProductId storeProductId,
        int diamondAmount,
        bool isAppleAvailable,
        bool isGoogleAvailable,
        int sortOrder)
    {
        Validate(diamondAmount, isAppleAvailable, isGoogleAvailable);

        return new PaymentProduct(
            new PaymentProductId(Guid.NewGuid()),
            storeProductId,
            diamondAmount,
            isAppleAvailable,
            isGoogleAvailable,
            sortOrder);
    }

    internal void Update(
        int diamondAmount,
        bool isAppleAvailable,
        bool isGoogleAvailable,
        int sortOrder)
    {
        Validate(diamondAmount, isAppleAvailable, isGoogleAvailable);

        _diamondAmount = diamondAmount;
        _isAppleAvailable = isAppleAvailable;
        _isGoogleAvailable = isGoogleAvailable;
        _sortOrder = sortOrder;

        AddDomainEvent(new PaymentProductUpdatedDomainEvent(Id.Value));
    }

    internal void Deactivate()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;

        AddDomainEvent(new PaymentProductDeactivatedDomainEvent(Id.Value));
    }

    public bool Supports(PaymentPlatform platform) =>
        platform switch
        {
            PaymentPlatform.Apple => _isAppleAvailable,
            PaymentPlatform.Google => _isGoogleAvailable,
            _ => false
        };

    private static void Validate(
        int diamondAmount,
        bool isAppleAvailable,
        bool isGoogleAvailable)
    {
        CheckRule(new AmountMustBePositiveRule(diamondAmount, nameof(diamondAmount)));
        CheckRule(new ProductMustSupportAtLeastOnePlatformRule(isAppleAvailable, isGoogleAvailable));
    }
}
