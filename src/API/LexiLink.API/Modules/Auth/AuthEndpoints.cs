using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.Common.Application;
using LexiLink.Modules.Players.Application.Contracts;
using LexiLink.Modules.Players.Application.Players.GetPlayerByAuthProvider;
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

                var token = tokenIssuer.Issue(player.Id);

                return Results.Ok(new TokenExchangeResponse(token.AccessToken, token.ExpiresAt, player.Id));
            })
            .AllowAnonymous()
            .Produces<TokenExchangeResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record TokenExchangeRequest(
    AuthProvider Provider,
    string ExternalId,
    string ExternalToken);

public sealed record TokenExchangeResponse(
    string AccessToken,
    DateTime ExpiresAt,
    Guid PlayerId);
