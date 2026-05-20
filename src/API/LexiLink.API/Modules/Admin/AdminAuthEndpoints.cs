using LexiLink.API.Configuration.Authentication;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.API.CrossModule;
using LexiLink.Common.Application;

namespace LexiLink.API.Modules.Admin;

public static class AdminAuthEndpoints
{
    public static void MapAdminAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth")
            .WithTags("Admin");

        authGroup.MapPost(
            "/admin/token",
            async (
                AdminTokenExchangeRequest body,
                IExternalAdminIdentityVerifier verifier,
                IAdminLookup adminLookup,
                IJwtTokenIssuer tokenIssuer,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.ExternalToken))
                {
                    var errors = new Dictionary<string, string[]>();
                    if (string.IsNullOrWhiteSpace(body.Email))
                    {
                        errors[nameof(AdminTokenExchangeRequest.Email)] = ["Email is required."];
                    }
                    if (string.IsNullOrWhiteSpace(body.ExternalToken))
                    {
                        errors[nameof(AdminTokenExchangeRequest.ExternalToken)] = ["ExternalToken is required."];
                    }
                    return Results.ValidationProblem(errors);
                }

                var verified = await verifier.VerifyAsync(body.Email, body.ExternalToken, ct);
                if (!verified)
                {
                    return Results.Unauthorized();
                }

                var admin = await adminLookup.FindActiveByEmailAsync(body.Email, ct);
                if (admin is null)
                {
                    return ApiProblemResults.NotFound(
                        httpContext,
                        $"No active admin user is registered for email '{body.Email}'.");
                }

                var token = tokenIssuer.IssueAdmin(admin.AdminUserId);
                return Results.Ok(new AdminTokenExchangeResponse(
                    token.AccessToken,
                    token.ExpiresAt,
                    admin.AdminUserId,
                    admin.Email,
                    admin.Role));
            })
            .AllowAnonymous()
            .Produces<AdminTokenExchangeResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        var adminGroup = app.MapGroup("/admin")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        adminGroup.MapGet(
            "/whoami",
            (IExecutionContextAccessor executionContext) =>
            {
                if (!executionContext.IsAdmin || executionContext.AdminUserId is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(new AdminWhoAmIResponse(
                    executionContext.AdminUserId.Value,
                    AuthConstants.AdminRoleValue));
            })
            .Produces<AdminWhoAmIResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}

public sealed record AdminTokenExchangeRequest(string Email, string ExternalToken);

public sealed record AdminTokenExchangeResponse(
    string AccessToken,
    DateTime ExpiresAt,
    Guid AdminUserId,
    string Email,
    string Role);

public sealed record AdminWhoAmIResponse(Guid AdminUserId, string Role);
