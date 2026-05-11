using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using LexiLink.Modules.Games.Application.Categories.EditCategory;
using LexiLink.Modules.Games.Application.Categories.GetCategories;
using LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.API.Modules.Games;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories").WithTags("Categories");

        group.MapPost("/", async (CreateCategoryRequest body, IGamesModule gamesModule, CancellationToken ct) =>
        {
            var id = await gamesModule.ExecuteCommandAsync(new CreateCategoryCommand(body.Name, body.Description), ct);
            return Results.Created($"/categories/{id}", new { id });
        });

        group.MapPatch("/{id:guid}", async (Guid id, EditCategoryRequest body, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new EditCategoryCommand(id, body.Name, body.Description), ct);
            return Results.NoContent();
        });

        group.MapGet("/", async (IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetCategoriesQuery(), ct)));

        group.MapGet("/{id:guid}", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetCategoryDetailsQuery(id), ct)));
    }
}

public record CreateCategoryRequest(string Name, string Description);
public record EditCategoryRequest(string Name, string Description);
