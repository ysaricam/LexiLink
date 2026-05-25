using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Hint.Application.Contracts;
using LexiLink.Modules.Hint.Application.PlayerHintInventories.GetPlayerHint;

namespace LexiLink.API.Modules.Hint;

public static class HintEndpoints
{
    public static IEndpointRouteBuilder MapHintEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/hint")
            .WithTags("Hint")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            IExecutionContextAccessor executionContextAccessor,
            IHintModule hintModule,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await hintModule.ExecuteQueryAsync(
                new GetPlayerHintQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(snapshot);
        });

        return app;
    }
}
