using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Reset.Application.Admin.GrantBonusReset;
using LexiLink.Modules.Reset.Application.Admin.ResetPlayerReset;
using LexiLink.Modules.Reset.Application.Admin.SetPlayerReset;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.GetPlayerReset;

namespace LexiLink.API.Modules.Admin;

public static class AdminResetEndpoints
{
    public static void MapAdminResetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players/{playerId:guid}/reset")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "",
            async (IResetModule reset, Guid playerId, CancellationToken ct) =>
            {
                var snapshot = await reset.ExecuteQueryAsync(
                    new GetPlayerResetQuery(playerId),
                    ct);
                return Results.Ok(snapshot);
            })
            .Produces<PlayerResetSnapshotDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/set",
            async (IResetModule reset, Guid playerId, SetResetRequest body, CancellationToken ct) =>
            {
                await reset.ExecuteCommandAsync(new SetPlayerResetCommand(playerId, body.Balance), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/grant",
            async (IResetModule reset, Guid playerId, GrantResetRequest body, CancellationToken ct) =>
            {
                await reset.ExecuteCommandAsync(new GrantBonusResetCommand(playerId, body.Amount), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/reset",
            async (IResetModule reset, Guid playerId, CancellationToken ct) =>
            {
                await reset.ExecuteCommandAsync(new ResetPlayerResetCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record SetResetRequest(int Balance);
public sealed record GrantResetRequest(int Amount);
