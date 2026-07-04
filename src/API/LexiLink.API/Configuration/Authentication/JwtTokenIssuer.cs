using System.Security.Claims;
using System.Text;
using LexiLink.Common.Application;
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

    public IssuedToken Issue(Guid playerId, PlayerAuthSessionMode sessionMode = PlayerAuthSessionMode.Guest) =>
        CreateToken(
            subject: playerId,
            extraClaims:
            [
                new Claim(AuthConstants.PlayerAuthSessionModeClaimType, sessionMode.ToString())
            ]);

    public IssuedToken IssueAdmin(Guid adminUserId) =>
        CreateToken(
            subject: adminUserId,
            extraClaims:
            [
                new Claim(AuthConstants.RoleClaimType, AuthConstants.AdminRoleValue),
                new Claim(AuthConstants.AdminUserIdClaimType, adminUserId.ToString())
            ]);

    private IssuedToken CreateToken(Guid subject, IEnumerable<Claim> extraClaims)
    {
        var now = _clock.UtcNow;
        var expiresAt = now.AddMinutes(_authOptions.Jwt.AccessTokenLifetimeMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Jwt.SigningKey!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.ToString())
        };
        claims.AddRange(extraClaims);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _authOptions.Jwt.Issuer,
            Audience = _authOptions.Jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            IssuedAt = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
