using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Undo.Application.Contracts;
using LexiLink.Modules.Undo.Application.PlayerUndoInventories.GetPlayerUndo;

namespace LexiLink.API.Modules.Undo;

public static class UndoEndpoints
{
    public static IEndpointRouteBuilder MapUndoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/undo")
            .WithTags("Undo")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            IExecutionContextAccessor executionContextAccessor,
            IUndoModule undoModule,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await undoModule.ExecuteQueryAsync(
                new GetPlayerUndoQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(snapshot);
        });

        return app;
    }
}
