namespace LexiLink.Modules.Players.Application.Players.GetPlayerById;

public record PlayerDetailsDto(
    Guid Id,
    string DisplayName,
    int Discriminator,
    string Handle,
    string? AvatarUrl,
    string Locale,
    bool IsGuest)
{
    public IReadOnlyList<AuthIdentityDto> AuthIdentities { get; init; } = [];
}
