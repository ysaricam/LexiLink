using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Reset.Application.Contracts;
using LexiLink.Modules.Reset.Application.PlayerResetInventories.GetPlayerReset;

namespace LexiLink.API.Modules.Reset;

public static class ResetEndpoints
{
    public static IEndpointRouteBuilder MapResetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reset")
            .WithTags("Reset")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            IExecutionContextAccessor executionContextAccessor,
            IResetModule resetModule,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await resetModule.ExecuteQueryAsync(
                new GetPlayerResetQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(snapshot);
        });

        return app;
    }
}
