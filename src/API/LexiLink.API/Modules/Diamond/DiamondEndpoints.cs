using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Diamond.Application.Contracts;
using LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GetPlayerDiamond;

namespace LexiLink.API.Modules.Diamond;

public static class DiamondEndpoints
{
    public static IEndpointRouteBuilder MapDiamondEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/diamond")
            .WithTags("Diamond")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            IExecutionContextAccessor executionContextAccessor,
            IDiamondModule diamondModule,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await diamondModule.ExecuteQueryAsync(
                new GetPlayerDiamondQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(snapshot);
        });

        return app;
    }
}
