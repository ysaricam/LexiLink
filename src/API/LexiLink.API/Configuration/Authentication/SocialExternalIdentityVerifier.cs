using System.Collections.Concurrent;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.API.Configuration.Authentication;

public sealed class SocialExternalIdentityVerifier : IExternalIdentityVerifier
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

    private readonly GuestExternalIdentityVerifier _guestVerifier = new();
    private readonly LexiLinkAuthOptions _options;
    private readonly HttpClient _httpClient;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly ConcurrentDictionary<AuthProvider, CachedKeys> _keyCache = new();
    private readonly SemaphoreSlim _keysLock = new(1, 1);

    public SocialExternalIdentityVerifier(LexiLinkAuthOptions options)
        : this(options, new HttpClient())
    {
    }

    internal SocialExternalIdentityVerifier(LexiLinkAuthOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyAsync(
        AuthProvider provider,
        string externalId,
        string externalToken,
        CancellationToken cancellationToken = default)
    {
        if (provider == AuthProvider.Guest)
        {
            return await _guestVerifier.VerifyAsync(provider, externalId, externalToken, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(externalId) || string.IsNullOrWhiteSpace(externalToken))
        {
            return false;
        }

        return provider switch
        {
            AuthProvider.Google => await VerifyJwtAsync(
                provider,
                externalId,
                externalToken,
                validIssuers: ["https://accounts.google.com", "accounts.google.com"],
                validAudiences: _options.SocialIdentity.GoogleClientIds,
                cancellationToken),
            AuthProvider.Apple => await VerifyJwtAsync(
                provider,
                externalId,
                externalToken,
                validIssuers: ["https://appleid.apple.com"],
                validAudiences: _options.SocialIdentity.AppleClientIds,
                cancellationToken),
            _ => false
        };
    }

    private async Task<bool> VerifyJwtAsync(
        AuthProvider provider,
        string externalId,
        string token,
        string[] validIssuers,
        string[] validAudiences,
        CancellationToken cancellationToken)
    {
        if (validAudiences.Length == 0)
        {
            return false;
        }

        try
        {
            var keys = await GetSigningKeysAsync(provider, cancellationToken);
            var result = await _tokenHandler.ValidateTokenAsync(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = validIssuers,
                    ValidateAudience = true,
                    ValidAudiences = validAudiences,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = keys,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                });

            if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt)
            {
                return false;
            }

            return string.Equals(jwt.Subject, externalId, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private async Task<IReadOnlyCollection<SecurityKey>> GetSigningKeysAsync(
        AuthProvider provider,
        CancellationToken cancellationToken)
    {
        if (_keyCache.TryGetValue(provider, out var cached)
            && cached.ExpiresAt > DateTimeOffset.UtcNow)
        {
            return cached.Keys;
        }

        await _keysLock.WaitAsync(cancellationToken);
        try
        {
            if (_keyCache.TryGetValue(provider, out cached)
                && cached.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return cached.Keys;
            }

            var jwksUri = provider switch
            {
                AuthProvider.Google => "https://www.googleapis.com/oauth2/v3/certs",
                AuthProvider.Apple => "https://appleid.apple.com/auth/keys",
                _ => throw new InvalidOperationException($"Unsupported social provider: {provider}")
            };

            var jwks = await _httpClient.GetStringAsync(jwksUri, cancellationToken);
            var keys = new JsonWebKeySet(jwks).Keys.Cast<SecurityKey>().ToArray();
            _keyCache[provider] = new CachedKeys(keys, DateTimeOffset.UtcNow.Add(CacheTtl));
            return keys;
        }
        finally
        {
            _keysLock.Release();
        }
    }

    private sealed record CachedKeys(IReadOnlyCollection<SecurityKey> Keys, DateTimeOffset ExpiresAt);
}
