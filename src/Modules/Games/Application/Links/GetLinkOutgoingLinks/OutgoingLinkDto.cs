namespace LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;

public record OutgoingLinkDto(
    Guid Id,
    string Value,
    bool IsActive);
