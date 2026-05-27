namespace LexiLink.Modules.Market.Application.Catalog;

public sealed record MarketCategoryDto(
    Guid Id,
    string Name,
    int SortOrder,
    string? Icon,
    bool IsActive,
    DateTime? VisibilityStartsAt,
    DateTime? VisibilityEndsAt,
    IReadOnlyList<MarketItemDto> Items);
