namespace LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;

public record CategoryDetailsDto(
    Guid Id,
    string Name,
    string Description,
    int LinkCount);
