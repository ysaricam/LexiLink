using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using LexiLink.API.CrossModule;
using LexiLink.Common.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LexiLink.API.Configuration.Authentication;

public sealed class LexiLinkBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly LexiLinkAuthOptions _authOptions;
    private readonly IAdminLookup _adminLookup;
    private readonly IPlayerStatusLookup _playerStatusLookup;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler = new();

    public LexiLinkBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        LexiLinkAuthOptions authOptions,
        IAdminLookup adminLookup,
        IPlayerStatusLookup playerStatusLookup)
        : base(options, logger, encoder)
    {
        _authOptions = authOptions;
        _adminLookup = adminLookup;
        _playerStatusLookup = playerStatusLookup;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return AuthenticateResult.NoResult();
        }

        const string bearerPrefix = "Bearer ";
        if (!authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Unsupported authorization scheme.");
        }

        var token = authorization[bearerPrefix.Length..].Trim();
        return _authOptions.Mode switch
        {
            LexiLinkAuthMode.DevelopmentBearer => await AuthenticateDevelopmentBearerAsync(token, Context.RequestAborted),
            LexiLinkAuthMode.ProductionJwt => await AuthenticateProductionJwtAsync(token, Context.RequestAborted),
            _ => AuthenticateResult.Fail($"Unsupported authentication mode '{_authOptions.Mode}'.")
        };
    }

    private async Task<AuthenticateResult> AuthenticateDevelopmentBearerAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(token, out var principalId))
        {
            return AuthenticateResult.Fail("Bearer token must be a player or admin id.");
        }

        // Banned players are refused at the auth boundary so they cannot
        // continue to invoke gameplay endpoints. Admin tokens are exempt —
        // an admin can be banned as a player but still log in as admin.
        var admin = await _adminLookup.FindActiveByIdAsync(principalId, cancellationToken);
        if (admin is null)
        {
            var isBanned = await _playerStatusLookup.IsPlayerBannedAsync(principalId, cancellationToken);
            if (isBanned)
            {
                return AuthenticateResult.Fail("Player is banned.");
            }
        }

        // Dev mode trusts any well-formed GUID as a player principal. If the
        // same GUID also matches an active admin in the Administration module
        // we attach the admin role claim so AuthenticatedAdmin endpoints
        // authorize the same token.
        var adminClaims = admin is null
            ? (IEnumerable<Claim>)
            [
                new Claim(AuthConstants.PlayerAuthSessionModeClaimType, PlayerAuthSessionMode.Guest.ToString())
            ]
            : (IEnumerable<Claim>)
            [
                new Claim(AuthConstants.RoleClaimType, AuthConstants.AdminRoleValue),
                new Claim(AuthConstants.AdminUserIdClaimType, admin.AdminUserId.ToString())
            ];

        return CreateTicket(principalId, adminClaims);
    }

    private async Task<AuthenticateResult> AuthenticateProductionJwtAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var signingKey = Encoding.UTF8.GetBytes(_authOptions.Jwt.SigningKey!);
        var result = await _jsonWebTokenHandler.ValidateTokenAsync(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _authOptions.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = _authOptions.Jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            });

        if (!result.IsValid)
        {
            return AuthenticateResult.Fail(result.Exception?.Message ?? "JWT validation failed.");
        }

        var sub = result.ClaimsIdentity.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value
            ?? result.ClaimsIdentity.Claims.SingleOrDefault(claim => claim.Type == "sub")?.Value;

        if (!Guid.TryParse(sub, out var principalId))
        {
            return AuthenticateResult.Fail("JWT subject must be a player or admin id.");
        }

        // If the JWT carries the admin role claim, re-verify against
        // Administration so a revoked or disabled admin cannot keep using
        // an unexpired token.
        var carriesAdminRole = result.ClaimsIdentity.Claims
            .Any(c => c.Type == AuthConstants.RoleClaimType && c.Value == AuthConstants.AdminRoleValue);

        if (carriesAdminRole)
        {
            var adminIdRaw = result.ClaimsIdentity.Claims
                .SingleOrDefault(c => c.Type == AuthConstants.AdminUserIdClaimType)?.Value;
            if (!Guid.TryParse(adminIdRaw, out var adminId))
            {
                return AuthenticateResult.Fail("Admin JWT must include the admin_id claim.");
            }

            var admin = await _adminLookup.FindActiveByIdAsync(adminId, cancellationToken);
            if (admin is null)
            {
                return AuthenticateResult.Fail("Admin account is disabled or missing.");
            }
        }
        else
        {
            // Non-admin JWT — refuse if the subject is a banned player.
            var isBanned = await _playerStatusLookup.IsPlayerBannedAsync(principalId, cancellationToken);
            if (isBanned)
            {
                return AuthenticateResult.Fail("Player is banned.");
            }
        }

        return CreateTicket(principalId, result.ClaimsIdentity.Claims);
    }

    private static AuthenticateResult CreateTicket(Guid principalId, IEnumerable<Claim> additionalClaims)
    {
        var claims = additionalClaims
            .Where(claim => claim.Type != "sub" && claim.Type != ClaimTypes.NameIdentifier)
            .Concat(
            [
                new Claim("sub", principalId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, principalId.ToString())
            ])
            .ToArray();

        var identity = new ClaimsIdentity(claims, AuthConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthConstants.Scheme);

        return AuthenticateResult.Success(ticket);
    }
}
