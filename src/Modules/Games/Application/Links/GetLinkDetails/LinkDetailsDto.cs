namespace LexiLink.Modules.Games.Application.Links.GetLinkDetails;

public record LinkDetailsDto(
    Guid Id,
    Guid CategoryId,
    string Value,
    string Description,
    bool IsActive);
