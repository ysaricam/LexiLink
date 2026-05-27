using LexiLink.Common.Domain;
using LexiLink.Modules.Market.Domain.Events;
using LexiLink.Modules.Market.Domain.Rules;

namespace LexiLink.Modules.Market.Domain;

public class PurchaseOrder : Entity, IAggregateRoot
{
    private const int IdempotencyKeyMaxLength = 128;

    public PurchaseOrderId Id { get; private set; }
    private Guid _playerId;
    private ShopItemId _shopItemId = null!;
    private ItemType _itemType;
    private int _quantity;
    private int _diamondsPaid;
    private DateTime _purchasedAt;
    private string _idempotencyKey = null!;

    public Guid PlayerId => _playerId;
    public ShopItemId ShopItemId => _shopItemId;
    public ItemType ItemType => _itemType;
    public int Quantity => _quantity;
    public int DiamondsPaid => _diamondsPaid;
    public DateTime PurchasedAt => _purchasedAt;
    public string IdempotencyKey => _idempotencyKey;

    private PurchaseOrder()
    {
        Id = null!;
    }

    private PurchaseOrder(
        PurchaseOrderId id,
        Guid playerId,
        ShopItemId shopItemId,
        ItemType itemType,
        int quantity,
        int diamondsPaid,
        DateTime purchasedAt,
        string idempotencyKey)
    {
        Id = id;
        _playerId = playerId;
        _shopItemId = shopItemId;
        _itemType = itemType;
        _quantity = quantity;
        _diamondsPaid = diamondsPaid;
        _purchasedAt = purchasedAt;
        _idempotencyKey = idempotencyKey;

        AddDomainEvent(new PurchaseOrderCreatedDomainEvent(
            Id.Value,
            _playerId,
            _shopItemId.Value,
            _itemType,
            _quantity,
            _diamondsPaid,
            _purchasedAt,
            _idempotencyKey));
    }

    internal static PurchaseOrder Create(
        Guid playerId,
        ShopItemId shopItemId,
        ItemType itemType,
        int quantity,
        int diamondsPaid,
        DateTime purchasedAt,
        string idempotencyKey)
    {
        CheckRule(new PositiveAmountRule(quantity, nameof(quantity)));
        CheckRule(new NonNegativeAmountRule(diamondsPaid, nameof(diamondsPaid)));
        CheckRule(new NameMustNotBeEmptyRule(idempotencyKey));
        CheckRule(new NameMustNotExceedMaxLengthRule(idempotencyKey.Trim(), IdempotencyKeyMaxLength));

        return new PurchaseOrder(
            new PurchaseOrderId(Guid.NewGuid()),
            playerId,
            shopItemId,
            itemType,
            quantity,
            diamondsPaid,
            purchasedAt,
            idempotencyKey.Trim());
    }
}
