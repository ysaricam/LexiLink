using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LexiLink.API.Configuration.Authentication;

public sealed class LexiLinkBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly LexiLinkAuthOptions _authOptions;
    private readonly JsonWebTokenHandler _jsonWebTokenHandler = new();

    public LexiLinkBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        LexiLinkAuthOptions authOptions)
        : base(options, logger, encoder)
    {
        _authOptions = authOptions;
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
            LexiLinkAuthMode.DevelopmentBearer => AuthenticateDevelopmentBearer(token),
            LexiLinkAuthMode.ProductionJwt => await AuthenticateProductionJwtAsync(token),
            _ => AuthenticateResult.Fail($"Unsupported authentication mode '{_authOptions.Mode}'.")
        };
    }

    private static AuthenticateResult AuthenticateDevelopmentBearer(string token)
    {
        if (!Guid.TryParse(token, out var playerId))
        {
            return AuthenticateResult.Fail("Bearer token must be a player id.");
        }

        return CreateTicket(playerId, []);
    }

    private async Task<AuthenticateResult> AuthenticateProductionJwtAsync(string token)
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

        if (!Guid.TryParse(sub, out var playerId))
        {
            return AuthenticateResult.Fail("JWT subject must be a player id.");
        }

        return CreateTicket(playerId, result.ClaimsIdentity.Claims);
    }

    private static AuthenticateResult CreateTicket(Guid playerId, IEnumerable<Claim> additionalClaims)
    {
        var claims = additionalClaims
            .Where(claim => claim.Type != "sub" && claim.Type != ClaimTypes.NameIdentifier)
            .Concat(
            [
                new Claim("sub", playerId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, playerId.ToString())
            ])
            .ToArray();

        var identity = new ClaimsIdentity(claims, AuthConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthConstants.Scheme);

        return AuthenticateResult.Success(ticket);
    }
}
