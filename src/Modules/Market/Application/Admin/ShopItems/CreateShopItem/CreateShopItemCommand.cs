using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Market.Application.Contracts;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.CreateShopItem;

public sealed class CreateShopItemCommand : CommandBase<Guid>, IAdminCommand
{
    public Guid CategoryId { get; }
    public ItemType ItemType { get; }
    public int Quantity { get; }
    public int Price { get; }
    public int? PromoPrice { get; }
    public DateTime? PromotionStartsAt { get; }
    public DateTime? PromotionEndsAt { get; }
    public int? MaxStock { get; }
    public int? PerPlayerLimit { get; }
    public PerPlayerLimitWindow PerPlayerLimitWindow { get; }

    public CreateShopItemCommand(
        Guid categoryId,
        ItemType itemType,
        int quantity,
        int price,
        int? promoPrice,
        DateTime? promotionStartsAt,
        DateTime? promotionEndsAt,
        int? maxStock,
        int? perPlayerLimit,
        PerPlayerLimitWindow perPlayerLimitWindow)
    {
        CategoryId = categoryId;
        ItemType = itemType;
        Quantity = quantity;
        Price = price;
        PromoPrice = promoPrice;
        PromotionStartsAt = promotionStartsAt;
        PromotionEndsAt = promotionEndsAt;
        MaxStock = maxStock;
        PerPlayerLimit = perPlayerLimit;
        PerPlayerLimitWindow = perPlayerLimitWindow;
    }

    public string AuditTargetType => "Market.ShopItem";
    public string? AuditTargetId => null;
}
