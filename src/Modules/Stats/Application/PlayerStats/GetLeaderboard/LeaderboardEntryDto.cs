namespace LexiLink.Modules.Stats.Application.PlayerStats.GetLeaderboard;

public sealed record LeaderboardEntryDto(
    Guid PlayerId,
    string? DisplayName,
    int? Discriminator,
    string? Handle,
    string? AvatarUrl,
    string? Locale,
    int GamesCompleted,
    int? BestScore,
    int TotalScore,
    DateTime? LastGameCompletedOn);
