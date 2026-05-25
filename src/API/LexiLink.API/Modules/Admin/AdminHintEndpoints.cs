using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Hint.Application.Admin.GrantBonusHint;
using LexiLink.Modules.Hint.Application.Admin.ResetPlayerHint;
using LexiLink.Modules.Hint.Application.Admin.SetPlayerHint;
using LexiLink.Modules.Hint.Application.Contracts;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.GetPlayerHint;

namespace LexiLink.API.Modules.Admin;

public static class AdminHintEndpoints
{
    public static void MapAdminHintEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players/{playerId:guid}/hint")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "",
            async (IHintModule hint, Guid playerId, CancellationToken ct) =>
            {
                var snapshot = await hint.ExecuteQueryAsync(
                    new GetPlayerHintQuery(playerId),
                    ct);
                return Results.Ok(snapshot);
            })
            .Produces<PlayerHintSnapshotDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/set",
            async (IHintModule hint, Guid playerId, SetHintRequest body, CancellationToken ct) =>
            {
                await hint.ExecuteCommandAsync(new SetPlayerHintCommand(playerId, body.Balance), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/grant",
            async (IHintModule hint, Guid playerId, GrantHintRequest body, CancellationToken ct) =>
            {
                await hint.ExecuteCommandAsync(new GrantBonusHintCommand(playerId, body.Amount), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/reset",
            async (IHintModule hint, Guid playerId, CancellationToken ct) =>
            {
                await hint.ExecuteCommandAsync(new ResetPlayerHintCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record SetHintRequest(int Balance);
public sealed record GrantHintRequest(int Amount);
