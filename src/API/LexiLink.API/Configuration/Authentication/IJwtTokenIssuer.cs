namespace LexiLink.API.Configuration.Authentication;

public interface IJwtTokenIssuer
{
    IssuedToken Issue(Guid playerId);
}
