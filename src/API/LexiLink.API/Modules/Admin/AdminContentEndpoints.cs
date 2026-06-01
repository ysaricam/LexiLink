using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using LexiLink.Modules.Games.Application.Categories.EditCategory;
using LexiLink.Modules.Games.Application.Categories.GetCategories;
using LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;
using LexiLink.Modules.Games.Application.Contracts;
using LexiLink.Modules.Games.Application.Links.ActivateLink;
using LexiLink.Modules.Games.Application.Links.AddOutgoingLink;
using LexiLink.Modules.Games.Application.Links.CreateLink;
using LexiLink.Modules.Games.Application.Links.DeactivateLink;
using LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;
using Microsoft.AspNetCore.Mvc;

namespace LexiLink.API.Modules.Admin;

/// <summary>
/// Content management endpoints (categories, links, edges) moved
/// behind <c>AuthenticatedAdmin</c> in Slice B10. Player-facing read
/// endpoints (GET /categories, GET /links, GET /links/{id}/outgoing)
/// stay on the <c>AuthenticatedPlayer</c> policy in
/// <see cref="LexiLink.API.Modules.Games.CategoryEndpoints"/> /
/// <see cref="LexiLink.API.Modules.Games.LinkEndpoints"/>.
/// </summary>
public static class AdminContentEndpoints
{
    public static void MapAdminContentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/content")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet("/categories", async (
            [FromQuery] string? locale, IGamesModule games, CancellationToken ct) =>
            Results.Ok(await games.ExecuteQueryAsync(new GetCategoriesQuery(locale), ct)))
            .Produces<IReadOnlyList<CategoryListItemDto>>();

        group.MapGet("/categories/{id:guid}", async (
            Guid id, IGamesModule games, CancellationToken ct) =>
            Results.Ok(await games.ExecuteQueryAsync(new GetCategoryDetailsQuery(id), ct)))
            .Produces<CategoryDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/categories", async (
            CreateCategoryRequest body, IGamesModule games, CancellationToken ct) =>
        {
            var id = await games.ExecuteCommandAsync(
                new CreateCategoryCommand(body.Name, body.Description, body.Language), ct);
            return Results.Created($"/admin/content/categories/{id}", new { id });
        })
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPatch("/categories/{id:guid}", async (
            Guid id, EditCategoryRequest body, IGamesModule games, CancellationToken ct) =>
        {
            await games.ExecuteCommandAsync(
                new EditCategoryCommand(id, body.Name, body.Description, body.Language), ct);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost("/links", async (
            CreateLinkRequest body, IGamesModule games, CancellationToken ct) =>
        {
            var id = await games.ExecuteCommandAsync(
                new CreateLinkCommand(body.CategoryId, body.Value, body.Description, body.IsActive), ct);
            return Results.Created($"/admin/content/links/{id}", new { id });
        })
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/links/{linkId:guid}/outgoing/{outgoingLinkId:guid}", async (
            Guid linkId, Guid outgoingLinkId, IGamesModule games, CancellationToken ct) =>
        {
            await games.ExecuteCommandAsync(new AddOutgoingLinkCommand(linkId, outgoingLinkId), ct);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/links/{linkId:guid}/outgoing/{outgoingLinkId:guid}", async (
            Guid linkId, Guid outgoingLinkId, IGamesModule games, CancellationToken ct) =>
        {
            await games.ExecuteCommandAsync(new RemoveOutgoingLinkCommand(linkId, outgoingLinkId), ct);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/links/{id:guid}/activate", async (
            Guid id, IGamesModule games, CancellationToken ct) =>
        {
            await games.ExecuteCommandAsync(new ActivateLinkCommand(id), ct);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/links/{id:guid}/deactivate", async (
            Guid id, IGamesModule games, CancellationToken ct) =>
        {
            await games.ExecuteCommandAsync(new DeactivateLinkCommand(id), ct);
            return Results.NoContent();
        })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record CreateCategoryRequest(string Name, string Description, string Language = "tr-TR");
public sealed record EditCategoryRequest(string Name, string Description, string Language = "tr-TR");
public sealed record CreateLinkRequest(Guid CategoryId, string Value, string Description, bool IsActive);
