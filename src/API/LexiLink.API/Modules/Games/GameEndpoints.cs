using LexiLink.Modules.Games.Application.Games.AbandonGame;
using LexiLink.Modules.Games.Application.Games.CreateGame;
using LexiLink.Modules.Games.Application.Games.GetGameById;
using LexiLink.Modules.Games.Application.Games.MakeStep;
using LexiLink.Modules.Games.Application.Games.Reset;
using LexiLink.Modules.Games.Application.Games.StartGame;
using LexiLink.Modules.Games.Application.Games.Undo;
using LexiLink.Modules.Games.Application.Games.UseHint;
using LexiLink.Modules.Games.Application.Contracts;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.API.Modules.Games;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Games");

        group.MapPost("/", async (CreateGameRequest body, IGamesModule gamesModule, CancellationToken ct) =>
        {
            var id = await gamesModule.ExecuteCommandAsync(
                new CreateGameCommand(body.PlayerId, body.CategoryId, body.Difficulty),
                ct);
            return Results.Created($"/games/{id}", new { id });
        });

        group.MapGet("/{id:guid}", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteQueryAsync(new GetGameByIdQuery(id), ct)));

        group.MapPost("/{id:guid}/start", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new StartGameCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/steps", async (
            Guid id, MakeStepRequest body, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new MakeStepCommand(id, body.NextLinkId), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/hint", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
            Results.Ok(await gamesModule.ExecuteCommandAsync(new UseHintCommand(id), ct)));

        group.MapPost("/{id:guid}/undo", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new UndoCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/reset", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new ResetCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/abandon", async (Guid id, IGamesModule gamesModule, CancellationToken ct) =>
        {
            await gamesModule.ExecuteCommandAsync(new AbandonGameCommand(id), ct);
            return Results.NoContent();
        });
    }
}

public record CreateGameRequest(Guid PlayerId, Guid CategoryId, Difficulty Difficulty);
public record MakeStepRequest(Guid NextLinkId);
