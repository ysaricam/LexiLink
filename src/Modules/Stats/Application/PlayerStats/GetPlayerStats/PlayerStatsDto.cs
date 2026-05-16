namespace LexiLink.Modules.Stats.Application.PlayerStats.GetPlayerStats;

public sealed record PlayerStatsDto(
    Guid PlayerId,
    string? DisplayName,
    int? Discriminator,
    string? Handle,
    string? AvatarUrl,
    string? Locale,
    bool IsGuest,
    int AuthProvidersLinked,
    int GamesCompleted,
    int? BestScore,
    int TotalScore,
    DateTime? LastGameCompletedOn,
    DateTime CreatedAt,
    DateTime UpdatedAt);
