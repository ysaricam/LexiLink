using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Energy.Application.Admin.GrantBonusEnergy;
using LexiLink.Modules.Energy.Application.Admin.ResetPlayerEnergy;
using LexiLink.Modules.Energy.Application.Admin.SetPlayerEnergy;
using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GetPlayerEnergy;

namespace LexiLink.API.Modules.Admin;

public static class AdminEnergyEndpoints
{
    public static void MapAdminEnergyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players/{playerId:guid}/energy")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "",
            async (IEnergyModule energy, Guid playerId, CancellationToken ct) =>
            {
                var snapshot = await energy.ExecuteQueryAsync(
                    new GetPlayerEnergyQuery(playerId),
                    ct);
                return Results.Ok(snapshot);
            })
            .Produces<PlayerEnergySnapshotDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/set",
            async (IEnergyModule energy, Guid playerId, SetEnergyRequest body, CancellationToken ct) =>
            {
                await energy.ExecuteCommandAsync(new SetPlayerEnergyCommand(playerId, body.Amount), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/grant",
            async (IEnergyModule energy, Guid playerId, GrantEnergyRequest body, CancellationToken ct) =>
            {
                await energy.ExecuteCommandAsync(new GrantBonusEnergyCommand(playerId, body.Amount), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/reset",
            async (IEnergyModule energy, Guid playerId, CancellationToken ct) =>
            {
                await energy.ExecuteCommandAsync(new ResetPlayerEnergyCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record SetEnergyRequest(int Amount);
public sealed record GrantEnergyRequest(int Amount);
