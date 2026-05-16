using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Energy.Application.Contracts;
using LexiLink.Modules.Energy.Application.PlayerEnergies.GetPlayerEnergy;

namespace LexiLink.API.Modules.Energy;

public static class EnergyEndpoints
{
    public static IEndpointRouteBuilder MapEnergyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/energy")
            .WithTags("Energy")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            IExecutionContextAccessor executionContextAccessor,
            IEnergyModule energyModule,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await energyModule.ExecuteQueryAsync(
                new GetPlayerEnergyQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(snapshot);
        });

        return app;
    }
}
