using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Orders;

public sealed record MarketOrderDto(
    Guid Id,
    Guid PlayerId,
    Guid ShopItemId,
    ItemType ItemType,
    int Quantity,
    int DiamondsPaid,
    DateTime PurchasedAt,
    string IdempotencyKey);
