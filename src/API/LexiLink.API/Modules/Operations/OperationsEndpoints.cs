using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.Operations;

namespace LexiLink.API.Modules.Operations;

public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/operations")
            .WithTags("Operations")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/processors", async (
                ProcessorBacklogReader backlogReader,
                CancellationToken cancellationToken) =>
            Results.Ok(await backlogReader.ReadAsync(cancellationToken)))
            .Produces<ProcessorBacklogResponse>();

        return app;
    }
}
