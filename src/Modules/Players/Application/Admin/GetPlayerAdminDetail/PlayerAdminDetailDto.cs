namespace LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;

public sealed record PlayerAdminDetailDto(
    Guid Id,
    string DisplayName,
    int Discriminator,
    string Handle,
    string? AvatarUrl,
    string Locale,
    bool IsGuest,
    bool IsBanned,
    string? BannedReason,
    DateTime? BannedAt,
    DateTime CreatedAt,
    int AuthProvidersLinked);
