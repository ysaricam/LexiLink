using LexiLink.Modules.Players.Application.Players.GetPlayerByAuthProvider;
using LexiLink.Modules.Players.Application.Players.GetPlayerById;
using LexiLink.Modules.Players.Application.Players.LinkAuthProvider;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;
using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Modules.Players;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/players").WithTags("Players");

        group.MapPost("/guest", async (RegisterGuestPlayerRequest body, IPlayersModule playersModule, CancellationToken ct) =>
        {
            var id = await playersModule.ExecuteCommandAsync(
                new RegisterGuestPlayerCommand(body.DeviceId, body.DisplayName, body.Locale), ct);
            return Results.Created($"/players/{id}", new { id });
        });

        group.MapPost("/{id:guid}/auth-providers",
            async (Guid id, LinkAuthProviderRequest body, IPlayersModule playersModule, CancellationToken ct) =>
        {
            await playersModule.ExecuteCommandAsync(
                new LinkAuthProviderCommand(id, body.Provider, body.ExternalId, body.Email), ct);
            return Results.NoContent();
        });

        group.MapPatch("/{id:guid}/profile",
            async (Guid id, UpdatePlayerProfileRequest body, IPlayersModule playersModule, CancellationToken ct) =>
        {
            await playersModule.ExecuteCommandAsync(
                new UpdatePlayerProfileCommand(id, body.AvatarUrl, body.Locale), ct);
            return Results.NoContent();
        });

        group.MapGet("/{id:guid}", async (Guid id, IPlayersModule playersModule, CancellationToken ct) =>
            Results.Ok(await playersModule.ExecuteQueryAsync(new GetPlayerByIdQuery(id), ct)));

        group.MapGet("/by-auth",
            async (AuthProvider provider, string externalId, IPlayersModule playersModule, CancellationToken ct) =>
        {
            var dto = await playersModule.ExecuteQueryAsync(new GetPlayerByAuthProviderQuery(provider, externalId), ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });
    }
}

public record RegisterGuestPlayerRequest(string DeviceId, string DisplayName, string Locale);
public record LinkAuthProviderRequest(AuthProvider Provider, string ExternalId, string? Email);
public record UpdatePlayerProfileRequest(string? AvatarUrl, string Locale);
