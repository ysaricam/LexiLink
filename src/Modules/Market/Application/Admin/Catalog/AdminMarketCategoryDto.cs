namespace LexiLink.Modules.Market.Application.Admin.Catalog;

public sealed record AdminMarketCategoryDto(
    Guid Id,
    string Name,
    int SortOrder,
    string? Icon,
    bool IsActive,
    DateTime? VisibilityStartsAt,
    DateTime? VisibilityEndsAt);
