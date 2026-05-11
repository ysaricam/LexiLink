using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Players.GetPlayerById;

public record AuthIdentityDto(
    AuthProvider Provider,
    string ExternalId,
    string? Email,
    DateTime LinkedAt);
