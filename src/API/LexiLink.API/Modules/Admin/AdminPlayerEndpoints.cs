using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.Modules.Players.Application.Admin.BanPlayer;
using LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;
using LexiLink.Modules.Players.Application.Admin.UnbanPlayer;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.API.Modules.Admin;

public static class AdminPlayerEndpoints
{
    public static void MapAdminPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "/{playerId:guid}",
            async (IPlayersModule players, Guid playerId, HttpContext ctx, CancellationToken cancellationToken) =>
            {
                var detail = await players.ExecuteQueryAsync(
                    new GetPlayerAdminDetailQuery(playerId),
                    cancellationToken);
                if (detail is null)
                {
                    return ApiProblemResults.NotFound(ctx, $"Player '{playerId}' was not found.");
                }
                return Results.Ok(detail);
            })
            .Produces<PlayerAdminDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/{playerId:guid}/ban",
            async (IPlayersModule players, Guid playerId, BanRequest body, CancellationToken ct) =>
            {
                await players.ExecuteCommandAsync(
                    new BanPlayerCommand(playerId, body.Reason),
                    ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/{playerId:guid}/unban",
            async (IPlayersModule players, Guid playerId, CancellationToken ct) =>
            {
                await players.ExecuteCommandAsync(new UnbanPlayerCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record BanRequest(string Reason);
