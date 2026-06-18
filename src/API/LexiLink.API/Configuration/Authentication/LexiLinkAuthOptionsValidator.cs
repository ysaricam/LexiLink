namespace LexiLink.API.Configuration.Authentication;

public static class LexiLinkAuthOptionsValidator
{
    public static void Validate(LexiLinkAuthOptions options, IHostEnvironment environment)
    {
        if (environment.IsProduction() && options.Mode == LexiLinkAuthMode.DevelopmentBearer)
        {
            throw new InvalidOperationException(
                "Development bearer authentication is not allowed in Production. " +
                "Configure a production identity provider before running LexiLink in Production.");
        }

        if (options.Mode == LexiLinkAuthMode.ProductionJwt)
        {
            if (string.IsNullOrWhiteSpace(options.Jwt.Issuer))
            {
                throw new InvalidOperationException("Authentication:Jwt:Issuer is required for ProductionJwt mode.");
            }

            if (string.IsNullOrWhiteSpace(options.Jwt.Audience))
            {
                throw new InvalidOperationException("Authentication:Jwt:Audience is required for ProductionJwt mode.");
            }

            if (string.IsNullOrWhiteSpace(options.Jwt.SigningKey))
            {
                throw new InvalidOperationException("Authentication:Jwt:SigningKey is required for ProductionJwt mode.");
            }

            if (options.Jwt.SigningKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "Authentication:Jwt:SigningKey must be at least 32 characters for ProductionJwt mode.");
            }
        }

        if (options.TokenExchange.Mode == ExternalIdentityValidationMode.DevelopmentExternalToken
            && environment.IsProduction())
        {
            throw new InvalidOperationException(
                "Development external identity validation is not allowed in Production.");
        }

        if (options.AdminTokenExchange.Mode == ExternalIdentityValidationMode.DevelopmentExternalToken
            && environment.IsProduction())
        {
            throw new InvalidOperationException(
                "Development external admin identity validation is not allowed in Production.");
        }

        if (options.AdminTokenExchange.Mode == ExternalIdentityValidationMode.AdminSharedSecret)
        {
            if (string.IsNullOrWhiteSpace(options.AdminTokenExchange.SharedSecret))
            {
                throw new InvalidOperationException(
                    "Authentication:AdminTokenExchange:SharedSecret is required for AdminSharedSecret mode.");
            }

            if (options.AdminTokenExchange.SharedSecret.Length < 32)
            {
                throw new InvalidOperationException(
                    "Authentication:AdminTokenExchange:SharedSecret must be at least 32 characters.");
            }
        }
    }
}
