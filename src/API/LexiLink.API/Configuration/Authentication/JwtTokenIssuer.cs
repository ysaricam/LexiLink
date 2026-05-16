using System.Security.Claims;
using System.Text;
using LexiLink.Common.Application.Time;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LexiLink.API.Configuration.Authentication;

public sealed class JwtTokenIssuer : IJwtTokenIssuer
{
    private readonly LexiLinkAuthOptions _authOptions;
    private readonly IClock _clock;

    public JwtTokenIssuer(LexiLinkAuthOptions authOptions, IClock clock)
    {
        _authOptions = authOptions;
        _clock = clock;
    }

    public IssuedToken Issue(Guid playerId)
    {
        var now = _clock.UtcNow;
        var expiresAt = now.AddMinutes(_authOptions.Jwt.AccessTokenLifetimeMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Jwt.SigningKey!));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _authOptions.Jwt.Issuer,
            Audience = _authOptions.Jwt.Audience,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, playerId.ToString())]),
            NotBefore = now,
            IssuedAt = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
