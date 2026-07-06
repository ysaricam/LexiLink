using LexiLink.API.CrossModule;
using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.Common.Application;
using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Application.Players.GetPlayerByAuthProvider;
using LexiLink.Modules.Players.Application.Players.LinkAuthProvider;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Modules.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapGet(
                "/me",
                (IExecutionContextAccessor executionContextAccessor) =>
                    Results.Ok(new { userId = executionContextAccessor.UserId }))
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy);

        group.MapPost(
            "/token",
            async (
                TokenExchangeRequest body,
                IExternalIdentityVerifier externalIdentityVerifier,
                IPlayersModule playersModule,
                IJwtTokenIssuer tokenIssuer,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var verified = await externalIdentityVerifier.VerifyAsync(
                    body.Provider,
                    body.ExternalId,
                    body.ExternalToken,
                    ct);

                if (!verified)
                {
                    return Results.Unauthorized();
                }

                var player = await playersModule.ExecuteQueryAsync(
                    new GetPlayerByAuthProviderQuery(body.Provider, body.ExternalId),
                    ct);

                if (player is null)
                {
                    return ApiProblemResults.NotFound(
                        httpContext,
                        $"Player with auth provider '{body.Provider}' and external id '{body.ExternalId}' was not found.");
                }

                var sessionMode = body.Provider == AuthProvider.Apple
                    ? PlayerAuthSessionMode.Apple
                    : PlayerAuthSessionMode.Guest;
                var token = tokenIssuer.Issue(player.Id, sessionMode);

                return Results.Ok(new TokenExchangeResponse(token.AccessToken, token.ExpiresAt, player.Id));
            })
            .AllowAnonymous()
            .Produces<TokenExchangeResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/apple/continue",
            async (
                AppleContinueRequest body,
                IExecutionContextAccessor executionContextAccessor,
                IExternalIdentityVerifier externalIdentityVerifier,
                IPlayersModule playersModule,
                IJwtTokenIssuer tokenIssuer,
                IPlayerStatusLookup playerStatusLookup,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var verified = await externalIdentityVerifier.VerifyAsync(
                    AuthProvider.Apple,
                    body.ExternalId,
                    body.ExternalToken,
                    ct);

                if (!verified)
                {
                    return Results.Unauthorized();
                }

                var currentPlayerId = executionContextAccessor.UserId;
                var existingApplePlayer = await playersModule.ExecuteQueryAsync(
                    new GetPlayerByAuthProviderQuery(AuthProvider.Apple, body.ExternalId),
                    ct);

                if (existingApplePlayer is not null)
                {
                    return await IssueAppleContinueTokenAsync(
                        existingApplePlayer.Id,
                        currentPlayerId,
                        tokenIssuer,
                        playerStatusLookup,
                        httpContext,
                        ct);
                }

                try
                {
                    await playersModule.ExecuteCommandAsync(
                        new LinkAuthProviderCommand(
                            currentPlayerId,
                            AuthProvider.Apple,
                            body.ExternalId,
                            body.Email),
                        ct);
                }
                catch (Exception ex) when (IsAuthProviderUniqueViolation(ex))
                {
                    existingApplePlayer = await playersModule.ExecuteQueryAsync(
                        new GetPlayerByAuthProviderQuery(AuthProvider.Apple, body.ExternalId),
                        ct);

                    if (existingApplePlayer is null)
                    {
                        throw;
                    }

                    return await IssueAppleContinueTokenAsync(
                        existingApplePlayer.Id,
                        currentPlayerId,
                        tokenIssuer,
                        playerStatusLookup,
                        httpContext,
                        ct);
                }

                var token = tokenIssuer.Issue(currentPlayerId, PlayerAuthSessionMode.Apple);

                return Results.Ok(new AppleContinueResponse(
                    token.AccessToken,
                    token.ExpiresAt,
                    currentPlayerId,
                    AppleContinueMode.LinkedCurrentGuest));
            })
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .Produces<AppleContinueResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> IssueAppleContinueTokenAsync(
        Guid applePlayerId,
        Guid currentPlayerId,
        IJwtTokenIssuer tokenIssuer,
        IPlayerStatusLookup playerStatusLookup,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (await playerStatusLookup.IsPlayerBannedAsync(applePlayerId, ct))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Player is banned.",
                instance: httpContext.Request.Path);
        }

        var token = tokenIssuer.Issue(applePlayerId, PlayerAuthSessionMode.Apple);
        var mode = applePlayerId == currentPlayerId
            ? AppleContinueMode.LinkedCurrentGuest
            : AppleContinueMode.SwitchedToExistingApplePlayer;

        return Results.Ok(new AppleContinueResponse(
            token.AccessToken,
            token.ExpiresAt,
            applePlayerId,
            mode));
    }

    private static bool IsAuthProviderUniqueViolation(Exception exception) =>
        DatabaseExceptionClassifier.IsPostgresUniqueViolation(
            exception,
            "UX_PlayerAuthIdentities_Provider_ExternalId");
}

public sealed record TokenExchangeRequest(
    AuthProvider Provider,
    string ExternalId,
    string ExternalToken);

public sealed record TokenExchangeResponse(
    string AccessToken,
    DateTime ExpiresAt,
    Guid PlayerId);

public sealed record AppleContinueRequest(
    string ExternalId,
    string ExternalToken,
    string? Email);

public sealed record AppleContinueResponse(
    string AccessToken,
    DateTime ExpiresAt,
    Guid PlayerId,
    AppleContinueMode Mode);

public enum AppleContinueMode
{
    LinkedCurrentGuest,
    SwitchedToExistingApplePlayer
}
