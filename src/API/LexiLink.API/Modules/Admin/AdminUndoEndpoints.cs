using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Undo.Application.Admin.GrantBonusUndo;
using LexiLink.Modules.Undo.Application.Admin.ResetPlayerUndo;
using LexiLink.Modules.Undo.Application.Admin.SetPlayerUndo;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

namespace LexiLink.API.Modules.Admin;

public static class AdminUndoEndpoints
{
    public static void MapAdminUndoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players/{playerId:guid}/undo")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "",
            async (IUndoModule undo, Guid playerId, CancellationToken ct) =>
            {
                var snapshot = await undo.ExecuteQueryAsync(
                    new GetPlayerUndoQuery(playerId, useGameplayPresentation: false),
                    ct);
                return Results.Ok(snapshot);
            })
            .Produces<PlayerUndoSnapshotDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/set",
            async (IUndoModule undo, Guid playerId, SetUndoRequest body, CancellationToken ct) =>
            {
                await undo.ExecuteCommandAsync(new SetPlayerUndoCommand(playerId, body.Balance), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/grant",
            async (IUndoModule undo, Guid playerId, GrantUndoRequest body, CancellationToken ct) =>
            {
                await undo.ExecuteCommandAsync(new GrantBonusUndoCommand(playerId, body.Amount), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/reset",
            async (IUndoModule undo, Guid playerId, CancellationToken ct) =>
            {
                await undo.ExecuteCommandAsync(new ResetPlayerUndoCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record SetUndoRequest(int Balance);
public sealed record GrantUndoRequest(int Amount);
