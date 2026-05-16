namespace LexiLink.API.Configuration.Authentication;

public sealed class LexiLinkAuthOptions
{
    public LexiLinkAuthMode Mode { get; set; } = LexiLinkAuthMode.DevelopmentBearer;

    public JwtAuthOptions Jwt { get; set; } = new();

    public TokenExchangeOptions TokenExchange { get; set; } = new();
}

public sealed class JwtAuthOptions
{
    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public string? SigningKey { get; set; }

    public int AccessTokenLifetimeMinutes { get; set; } = 60;
}

public sealed class TokenExchangeOptions
{
    public ExternalIdentityValidationMode Mode { get; set; } = ExternalIdentityValidationMode.Disabled;
}
