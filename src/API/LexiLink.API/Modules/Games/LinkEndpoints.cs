using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Games.Application.Links.ActivateLink;
using LexiLink.Modules.Games.Application.Links.AddOutgoingLink;
using LexiLink.Modules.Games.Application.Links.CreateLink;
using LexiLink.Modules.Games.Application.Links.DeactivateLink;
using LexiLink.Modules.Games.Application.Links.GetLinkDetails;
using LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;
using LexiLink.Modules.Games.Application.Links.GetLinksByCategory;
using LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.API.Modules.Games;

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

        group.MapPost("/", async (CreateLinkRequest body, IGamesModule gamesModule, CancellationToken ct) =>
        {
            var id = await gamesModule.ExecuteCommandAsync(
                new CreateLinkCommand(body.CategoryId, body.Value, body.Description, body.IsActive),
                ct);
            return Results.Created($"/links/{id}", new { id });
        });

        group.MapPost("/{linkId:guid}/outgoing/{outgoingLinkId:guid}", async (
            Guid linkId, Guid outgoingLinkId, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new AddOutgoingLinkCommand(linkId, outgoingLinkId), ct);
            return Results.NoContent();
        });

        group.MapDelete("/{linkId:guid}/outgoing/{outgoingLinkId:guid}", async (
            Guid linkId, Guid outgoingLinkId, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new RemoveOutgoingLinkCommand(linkId, outgoingLinkId), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/activate", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new ActivateLinkCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/deactivate", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new DeactivateLinkCommand(id), ct);
            return Results.NoContent();
        });

        group.MapGet("/{id:guid}", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetLinkDetailsQuery(id), ct)));

        group.MapGet("/{id:guid}/outgoing", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetLinkOutgoingLinksQuery(id), ct)));

        group.MapGet("/", async (Guid categoryId, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetLinksByCategoryQuery(categoryId), ct)));
    }
}

public record CreateLinkRequest(Guid CategoryId, string Value, string Description, bool IsActive);
