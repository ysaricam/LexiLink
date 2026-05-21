using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Games.Application.Contracts;
using LexiLink.Modules.Games.Application.Links.GetLinkDetails;
using LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;
using LexiLink.Modules.Games.Application.Links.GetLinksByCategory;

namespace LexiLink.API.Modules.Games;

/// <summary>
/// Player-facing read endpoints for links. Mutating endpoints
/// (create / activate / deactivate, add / remove edges) moved to
/// <c>/admin/content/links</c> in Slice B10.
/// </summary>
public static class LinkEndpoints
{
    public static void MapLinkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/links")
            .WithTags("Links")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetLinkDetailsQuery(id), ct)));

        group.MapGet("/{id:guid}/outgoing", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetLinkOutgoingLinksQuery(id), ct)));

        group.MapGet("/", async (Guid categoryId, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetLinksByCategoryQuery(categoryId), ct)));
    }
}
