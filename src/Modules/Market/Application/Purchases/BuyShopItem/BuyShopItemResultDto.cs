namespace LexiLink.Modules.Market.Application.Purchases.BuyShopItem;

public sealed record BuyShopItemResultDto(
    Guid PurchaseOrderId,
    Guid ShopItemId,
    string ItemType,
    int Quantity,
    int DiamondsPaid,
    DateTime PurchasedAt,
    bool IsReplay);
