using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Catalog;

public sealed record MarketItemDto(
    Guid Id,
    Guid CategoryId,
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
    int? PerPlayerRemaining,
    bool IsActive);
