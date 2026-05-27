using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Diamond.Application.Admin.GrantBonusDiamond;
using LexiLink.Modules.Diamond.Application.Admin.ResetPlayerDiamond;
using LexiLink.Modules.Diamond.Application.Admin.SetPlayerDiamond;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GetPlayerDiamond;

namespace LexiLink.API.Modules.Admin;

public static class AdminDiamondEndpoints
{
    public static void MapAdminDiamondEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players/{playerId:guid}/diamond")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "",
            async (IDiamondModule diamond, Guid playerId, CancellationToken ct) =>
            {
                var snapshot = await diamond.ExecuteQueryAsync(
                    new GetPlayerDiamondQuery(playerId),
                    ct);
                return Results.Ok(snapshot);
            })
            .Produces<PlayerDiamondSnapshotDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/set",
            async (IDiamondModule diamond, Guid playerId, SetDiamondRequest body, CancellationToken ct) =>
            {
                await diamond.ExecuteCommandAsync(new SetPlayerDiamondCommand(playerId, body.Balance), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/grant",
            async (IDiamondModule diamond, Guid playerId, GrantDiamondRequest body, CancellationToken ct) =>
            {
                await diamond.ExecuteCommandAsync(new GrantBonusDiamondCommand(playerId, body.Amount), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/reset",
            async (IDiamondModule diamond, Guid playerId, CancellationToken ct) =>
            {
                await diamond.ExecuteCommandAsync(new ResetPlayerDiamondCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record SetDiamondRequest(int Balance);
public sealed record GrantDiamondRequest(int Amount);
