using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Games.Application.Categories.GetCategories;
using LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;
using LexiLink.Modules.Games.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LexiLink.API.Modules.Games;

/// <summary>
/// Player-facing read endpoints for categories. Mutating endpoints
/// (create / edit) moved to <c>/admin/content/categories</c> in
/// Slice B10 — those are admin-only and audited.
/// </summary>
public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories")
            .WithTags("Categories")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", async (
            [FromQuery] string? locale,
            IGamesModule gamesModule,
            CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetCategoriesQuery(locale), ct)));

        group.MapGet("/{id:guid}", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetCategoryDetailsQuery(id), ct)));
    }
}
