using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.Catalog;

public sealed record AdminMarketItemDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    ItemType ItemType,
    int Quantity,
    int Price,
    int EffectivePrice,
    int? PromoPrice,
    DateTime? PromotionStartsAt,
    DateTime? PromotionEndsAt,
    int? MaxStock,
    int SoldCount,
    int? RemainingStock,
    int? PerPlayerLimit,
    PerPlayerLimitWindow PerPlayerLimitWindow,
    bool IsActive,
    uint Version);
