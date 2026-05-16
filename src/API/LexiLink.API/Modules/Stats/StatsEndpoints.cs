using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.Modules.Stats.Application.Contracts;
using LexiLink.Modules.Stats.Application.PlayerStats.GetLeaderboard;
using LexiLink.Modules.Stats.Application.PlayerStats.GetPlayerStats;

namespace LexiLink.API.Modules.Stats;

public static class StatsEndpoints
{
    public static IEndpointRouteBuilder MapStatsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/stats")
            .WithTags("Stats")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/players/{playerId:guid}", async (
            Guid playerId,
            IStatsModule statsModule,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var stats = await statsModule.ExecuteQueryAsync(
                new GetPlayerStatsQuery(playerId),
                cancellationToken);

            return stats is null
                ? ApiProblemResults.NotFound(httpContext, $"Stats for player '{playerId}' were not found.")
                : Results.Ok(stats);
        });

        group.MapGet("/leaderboard", async (
            string? orderBy,
            string? period,
            string? periodStart,
            int? limit,
            IStatsModule statsModule,
            CancellationToken cancellationToken) =>
        {
            var parsedOrderBy = Enum.TryParse<LeaderboardOrderBy>(
                orderBy,
                ignoreCase: true,
                out var requestedOrderBy)
                ? requestedOrderBy
                : LeaderboardOrderBy.BestScore;

            var parsedPeriod = Enum.TryParse<LeaderboardPeriod>(
                period,
                ignoreCase: true,
                out var requestedPeriod)
                ? requestedPeriod
                : LeaderboardPeriod.AllTime;

            var parsedPeriodStart = DateTime.TryParse(periodStart, out var requestedPeriodStart)
                ? requestedPeriodStart.Date
                : (DateTime?)null;

            var entries = await statsModule.ExecuteQueryAsync(
                new GetLeaderboardQuery(parsedOrderBy, limit ?? 50, parsedPeriod, parsedPeriodStart),
                cancellationToken);

            return Results.Ok(entries);
        });

        return app;
    }
}
