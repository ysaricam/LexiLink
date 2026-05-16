namespace LexiLink.API.Configuration.Authentication;

public sealed record IssuedToken(string AccessToken, DateTime ExpiresAt);
