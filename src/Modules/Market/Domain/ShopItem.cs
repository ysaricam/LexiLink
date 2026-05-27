using LexiLink.Common.Domain;
using LexiLink.Modules.Market.Domain.Events;
using LexiLink.Modules.Market.Domain.Rules;

namespace LexiLink.Modules.Market.Domain;

public class ShopItem : Entity, IAggregateRoot
{
    public ShopItemId Id { get; private set; }
    private CategoryId _categoryId = null!;
    private ItemType _itemType;
    private int _quantity;
    private int _price;
    private Promotion? _promotion;
    private int? _maxStock;
    private int _soldCount;
    private int? _perPlayerLimit;
    private PerPlayerLimitWindow _perPlayerLimitWindow;
    private bool _isActive;

    public CategoryId CategoryId => _categoryId;
    public ItemType ItemType => _itemType;
    public int Quantity => _quantity;
    public int Price => _price;
    public Promotion? Promotion => _promotion;
    public int? MaxStock => _maxStock;
    public int SoldCount => _soldCount;
    public int? PerPlayerLimit => _perPlayerLimit;
    public PerPlayerLimitWindow PerPlayerLimitWindow => _perPlayerLimitWindow;
    public bool IsActive => _isActive;

    // EF optimistic concurrency token. PostgreSQL compares this value in UPDATE predicates.
    public uint Version { get; private set; }

    private ShopItem()
    {
        Id = null!;
    }

    private ShopItem(
        ShopItemId id,
        CategoryId categoryId,
        ItemType itemType,
        int quantity,
        int price,
        Promotion? promotion,
        int? maxStock,
        int? perPlayerLimit,
        PerPlayerLimitWindow perPlayerLimitWindow)
    {
        Id = id;
        _categoryId = categoryId;
        _itemType = itemType;
        _quantity = quantity;
        _price = price;
        _promotion = promotion;
        _maxStock = maxStock;
        _perPlayerLimit = perPlayerLimit;
        _perPlayerLimitWindow = perPlayerLimitWindow;
        _soldCount = 0;
        _isActive = true;

        AddDomainEvent(new ShopItemCreatedDomainEvent(Id.Value, _categoryId.Value));
    }

    internal static ShopItem Create(
        CategoryId categoryId,
        ItemType itemType,
        int quantity,
        int price,
        Promotion? promotion,
        int? maxStock,
        int? perPlayerLimit,
        PerPlayerLimitWindow perPlayerLimitWindow)
    {
        Validate(quantity, price, maxStock, perPlayerLimit);

        return new ShopItem(
            new ShopItemId(Guid.NewGuid()),
            categoryId,
            itemType,
            quantity,
            price,
            promotion,
            maxStock,
            perPlayerLimit,
            perPlayerLimitWindow);
    }

    internal void Update(
        CategoryId categoryId,
        ItemType itemType,
        int quantity,
        int price,
        Promotion? promotion,
        int? maxStock,
        int? perPlayerLimit,
        PerPlayerLimitWindow perPlayerLimitWindow)
    {
        Validate(quantity, price, maxStock, perPlayerLimit);

        _categoryId = categoryId;
        _itemType = itemType;
        _quantity = quantity;
        _price = price;
        _promotion = promotion;
        _maxStock = maxStock;
        _perPlayerLimit = perPlayerLimit;
        _perPlayerLimitWindow = perPlayerLimitWindow;
        TouchVersion();

        AddDomainEvent(new ShopItemUpdatedDomainEvent(Id.Value));
    }

    internal void Deactivate()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;
        TouchVersion();

        AddDomainEvent(new ShopItemDeactivatedDomainEvent(Id.Value));
    }

    internal void RecordPurchase()
    {
        _soldCount++;
        TouchVersion();
    }

    public int EffectivePriceAt(DateTime now) =>
        _promotion is not null && _promotion.IsOpenAt(now)
            ? _promotion.PromoPrice
            : _price;

    public bool HasStockRemaining() => _maxStock is null || _soldCount < _maxStock.Value;

    private void TouchVersion() => Version++;

    private static void Validate(
        int quantity,
        int price,
        int? maxStock,
        int? perPlayerLimit)
    {
        CheckRule(new PositiveAmountRule(quantity, nameof(quantity)));
        CheckRule(new PositiveAmountRule(price, nameof(price)));
        CheckRule(new MaxStockMustBePositiveRule(maxStock));
        if (perPlayerLimit is not null)
        {
            CheckRule(new PositiveAmountRule(perPlayerLimit.Value, nameof(perPlayerLimit)));
        }
    }
}
