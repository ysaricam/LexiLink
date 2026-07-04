using Dapper;
using LexiLink.Common.Application.Data;
using LexiLink.Modules.Games.IntegrationEvents;
using LexiLink.Modules.Players.IntegrationEvents;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

namespace LexiLink.Modules.Stats.Infrastructure.Queries;

internal class PlayerStatsProjectionUpdater : IPlayerStatsProjectionUpdater
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    internal PlayerStatsProjectionUpdater(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public Task ProjectAsync(
        PlayerRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async connection =>
        {
            await connection.ExecuteAsync(
                    """
                    INSERT INTO "stats"."PlayerStats"
                        ("PlayerId", "DisplayName", "Discriminator", "AvatarUrl", "Locale", "IsGuest", "AuthProvidersLinked",
                         "GamesCompleted", "BestScore", "TotalScore", "CreatedAt", "UpdatedAt")
                    VALUES
                        (@PlayerId, @DisplayName, @Discriminator, NULL, @Locale, @IsGuest, 0,
                         0, NULL, 0, @OccurredOn, @OccurredOn)
                    ON CONFLICT ("PlayerId") DO UPDATE
                    SET "DisplayName" = EXCLUDED."DisplayName",
                        "Discriminator" = EXCLUDED."Discriminator",
                        "Locale" = EXCLUDED."Locale",
                        "IsGuest" = EXCLUDED."IsGuest",
                        "UpdatedAt" = EXCLUDED."UpdatedAt"
                    """,
                integrationEvent);
        });

    public Task ProjectAsync(
        AuthProviderLinkedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async connection =>
        {
            await connection.ExecuteAsync(
                    """
                    INSERT INTO "stats"."PlayerStats"
                        ("PlayerId", "IsGuest", "AuthProvidersLinked", "GamesCompleted",
                         "BestScore", "TotalScore", "CreatedAt", "UpdatedAt")
                    VALUES
                        (@PlayerId, false, 1, 0, NULL, 0, @OccurredOn, @OccurredOn)
                    ON CONFLICT ("PlayerId") DO UPDATE
                    SET "IsGuest" = false,
                        "AuthProvidersLinked" = "stats"."PlayerStats"."AuthProvidersLinked" + 1,
                        "UpdatedAt" = EXCLUDED."UpdatedAt"
                    """,
                integrationEvent);
        });

    public Task ProjectAsync(
        PlayerProfileUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async connection =>
        {
            await connection.ExecuteAsync(
                    """
                    INSERT INTO "stats"."PlayerStats"
                        ("PlayerId", "DisplayName", "Discriminator", "AvatarUrl", "Locale", "IsGuest", "AuthProvidersLinked",
                         "GamesCompleted", "BestScore", "TotalScore", "CreatedAt", "UpdatedAt")
                    VALUES
                        (@PlayerId, @DisplayName, @Discriminator, @AvatarUrl, @Locale, true, 0,
                         0, NULL, 0, @OccurredOn, @OccurredOn)
                    ON CONFLICT ("PlayerId") DO UPDATE
                    SET "DisplayName" = COALESCE(EXCLUDED."DisplayName", "stats"."PlayerStats"."DisplayName"),
                        "Discriminator" = COALESCE(EXCLUDED."Discriminator", "stats"."PlayerStats"."Discriminator"),
                        "AvatarUrl" = EXCLUDED."AvatarUrl",
                        "Locale" = EXCLUDED."Locale",
                        "UpdatedAt" = EXCLUDED."UpdatedAt"
                    """,
                integrationEvent);
        });

    public Task ProjectAsync(
        GameCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        ExecuteAsync(async connection =>
        {
            await connection.ExecuteAsync(
                    """
                    INSERT INTO "stats"."PlayerStats"
                        ("PlayerId", "IsGuest", "AuthProvidersLinked", "GamesCompleted",
                         "BestScore", "TotalScore", "LastGameCompletedOn", "CreatedAt", "UpdatedAt")
                    VALUES
                        (@PlayerId, true, 0, 1, @Score, @Score, @OccurredOn, @OccurredOn, @OccurredOn)
                    ON CONFLICT ("PlayerId") DO UPDATE
                    SET "GamesCompleted" = "stats"."PlayerStats"."GamesCompleted" + 1,
                        "BestScore" = GREATEST(COALESCE("stats"."PlayerStats"."BestScore", @Score), @Score),
                        "TotalScore" = "stats"."PlayerStats"."TotalScore" + @Score,
                        "LastGameCompletedOn" = @OccurredOn,
                        "UpdatedAt" = @OccurredOn
                    """,
                integrationEvent);

            var completedOnDate = integrationEvent.OccurredOn.Date;
            var periodRows = new[]
            {
                new PeriodStatsProjectionRow("Daily", completedOnDate, integrationEvent),
                new PeriodStatsProjectionRow("Weekly", GetWeekStartDate(completedOnDate), integrationEvent)
            };

            await connection.ExecuteAsync(
                    """
                    INSERT INTO "stats"."PlayerPeriodStats"
                        ("PeriodType", "PeriodStartDate", "PlayerId", "GamesCompleted",
                         "BestScore", "TotalScore", "LastGameCompletedOn", "CreatedAt", "UpdatedAt")
                    VALUES
                        (@PeriodType, @PeriodStartDate, @PlayerId, 1,
                         @Score, @Score, @OccurredOn, @OccurredOn, @OccurredOn)
                    ON CONFLICT ("PeriodType", "PeriodStartDate", "PlayerId") DO UPDATE
                    SET "GamesCompleted" = "stats"."PlayerPeriodStats"."GamesCompleted" + 1,
                        "BestScore" = GREATEST(COALESCE("stats"."PlayerPeriodStats"."BestScore", @Score), @Score),
                        "TotalScore" = "stats"."PlayerPeriodStats"."TotalScore" + @Score,
                        "LastGameCompletedOn" = @OccurredOn,
                        "UpdatedAt" = @OccurredOn
                    """,
                periodRows);
        });

    private async Task ExecuteAsync(Func<System.Data.IDbConnection, Task> project)
    {
        using var connection = _sqlConnectionFactory.CreateNewConnection();
        await project(connection);
    }

    private static DateTime GetWeekStartDate(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    private sealed record PeriodStatsProjectionRow(
        string PeriodType,
        DateTime PeriodStartDate,
        Guid PlayerId,
        int Score,
        DateTime OccurredOn)
    {
        public PeriodStatsProjectionRow(
            string periodType,
            DateTime periodStartDate,
            GameCompletedIntegrationEvent integrationEvent)
            : this(
                periodType,
                periodStartDate,
                integrationEvent.PlayerId,
                integrationEvent.Score,
                integrationEvent.OccurredOn)
        {
        }
    }
}
