using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.Modules.Players.Application.Admin.BanPlayer;
using LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;
using LexiLink.Modules.Players.Application.Admin.UnbanPlayer;
using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Domain.Players;
using Microsoft.AspNetCore.Mvc;

namespace LexiLink.API.Modules.Admin;

public static class AdminPlayerEndpoints
{
    public static void MapAdminPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/players")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "/by-handle",
            async (
                IPlayersModule players,
                [FromQuery] string handle,
                HttpContext ctx,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseHandle(handle, out var displayName, out var discriminator))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["handle"] = ["Handle must be in the format DisplayName#1234."]
                    });
                }

                var detail = await players.ExecuteQueryAsync(
                    new GetPlayerAdminDetailByHandleQuery(displayName, discriminator),
                    cancellationToken);
                if (detail is null)
                {
                    return ApiProblemResults.NotFound(ctx, $"Player '{handle}' was not found.");
                }
                return Results.Ok(detail);
            })
            .Produces<PlayerAdminDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapGet(
            "/{playerId:guid}",
            async (IPlayersModule players, Guid playerId, HttpContext ctx, CancellationToken cancellationToken) =>
            {
                var detail = await players.ExecuteQueryAsync(
                    new GetPlayerAdminDetailQuery(playerId),
                    cancellationToken);
                if (detail is null)
                {
                    return ApiProblemResults.NotFound(ctx, $"Player '{playerId}' was not found.");
                }
                return Results.Ok(detail);
            })
            .Produces<PlayerAdminDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/{playerId:guid}/ban",
            async (IPlayersModule players, Guid playerId, BanRequest body, CancellationToken ct) =>
            {
                await players.ExecuteCommandAsync(
                    new BanPlayerCommand(playerId, body.Reason),
                    ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/{playerId:guid}/unban",
            async (IPlayersModule players, Guid playerId, CancellationToken ct) =>
            {
                await players.ExecuteCommandAsync(new UnbanPlayerCommand(playerId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static bool TryParseHandle(string? handle, out string displayName, out int discriminator)
    {
        displayName = string.Empty;
        discriminator = 0;

        if (string.IsNullOrWhiteSpace(handle))
        {
            return false;
        }

        var separatorIndex = handle.LastIndexOf('#');
        if (separatorIndex <= 0 || separatorIndex == handle.Length - 1)
        {
            return false;
        }

        var discriminatorText = handle[(separatorIndex + 1)..];
        if (discriminatorText.Length > 4
            || !discriminatorText.All(char.IsDigit)
            || !int.TryParse(discriminatorText, out discriminator)
            || discriminator < Discriminator.MinValue
            || discriminator > Discriminator.MaxValue)
        {
            return false;
        }

        displayName = handle[..separatorIndex].Trim();
        return displayName.Length > 0;
    }
}

public sealed record BanRequest(string Reason);
