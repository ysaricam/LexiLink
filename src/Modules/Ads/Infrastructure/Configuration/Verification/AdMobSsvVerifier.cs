using LexiLink.Modules.Ads.Application.Configuration.Verification;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LexiLink.Modules.Ads.Infrastructure.Configuration.Verification;

/// <summary>
/// Verifies AdMob SSV callbacks against Google's rotating ECDSA public keys.
/// </summary>
public sealed class AdMobSsvVerifier : IAdMobSsvVerifier
{
    private static readonly TimeSpan PublicKeyCacheDuration = TimeSpan.FromHours(24);

    private readonly AdsSsvOptions _options;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _keysLock = new(1, 1);
    private IReadOnlyDictionary<string, ECDsa>? _cachedKeys;
    private DateTimeOffset _cachedKeysUntil;

    public AdMobSsvVerifier(AdsSsvOptions options)
        : this(options, new HttpClient())
    {
    }

    public AdMobSsvVerifier(AdsSsvOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public async Task<AdMobSsvVerificationResult> VerifyAsync(
        AdMobSsvVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SignedContent) ||
            string.IsNullOrWhiteSpace(request.Signature) ||
            string.IsNullOrWhiteSpace(request.KeyId))
        {
            return AdMobSsvVerificationResult.Failed("AdMob SSV callback is missing signed content, signature or key id.");
        }

        try
        {
            var keys = await GetPublicKeysAsync(cancellationToken);
            if (!keys.TryGetValue(request.KeyId, out var publicKey))
            {
                return AdMobSsvVerificationResult.Failed($"AdMob SSV key id '{request.KeyId}' is not trusted.");
            }

            var signedContent = Encoding.UTF8.GetBytes(request.SignedContent);
            var signature = DecodeBase64Url(request.Signature);
            var isVerified = publicKey.VerifyData(
                signedContent,
                signature,
                HashAlgorithmName.SHA256,
                DSASignatureFormat.Rfc3279DerSequence);

            return isVerified
                ? AdMobSsvVerificationResult.Verified()
                : AdMobSsvVerificationResult.Failed("AdMob SSV signature verification failed.");
        }
        catch (Exception ex) when (ex is HttpRequestException ||
                                   ex is JsonException ||
                                   ex is CryptographicException ||
                                   ex is FormatException ||
                                   ex is InvalidOperationException)
        {
            return AdMobSsvVerificationResult.Failed("AdMob SSV verification failed: " + ex.Message);
        }
    }

    private async Task<IReadOnlyDictionary<string, ECDsa>> GetPublicKeysAsync(CancellationToken cancellationToken)
    {
        if (_cachedKeys is not null && DateTimeOffset.UtcNow < _cachedKeysUntil)
        {
            return _cachedKeys;
        }

        await _keysLock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedKeys is not null && DateTimeOffset.UtcNow < _cachedKeysUntil)
            {
                return _cachedKeys;
            }

            using var response = await _httpClient.GetAsync(_options.VerificationKeysUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<AdMobPublicKeysPayload>(
                content,
                JsonSerializerOptions.Web,
                cancellationToken);

            var keys = payload?.Keys
                .Select(key => new
                {
                    KeyId = key.KeyIdText,
                    key.Base64
                })
                .Where(key => !string.IsNullOrWhiteSpace(key.KeyId) && !string.IsNullOrWhiteSpace(key.Base64))
                .ToDictionary(
                    key => key.KeyId,
                    key =>
                    {
                        var ecdsa = ECDsa.Create();
                        var subjectPublicKeyInfo = Convert.FromBase64String(key.Base64);
                        ecdsa.ImportSubjectPublicKeyInfo(subjectPublicKeyInfo, out _);
                        return ecdsa;
                    },
                    StringComparer.Ordinal);

            if (keys is null || keys.Count == 0)
            {
                throw new InvalidOperationException("AdMob public key server returned no usable keys.");
            }

            _cachedKeys = keys;
            _cachedKeysUntil = DateTimeOffset.UtcNow.Add(PublicKeyCacheDuration);
            return _cachedKeys;
        }
        finally
        {
            _keysLock.Release();
        }
    }

    private static byte[] DecodeBase64Url(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
        return Convert.FromBase64String(padded);
    }

    private sealed class AdMobPublicKeysPayload
    {
        public IReadOnlyList<AdMobPublicKey> Keys { get; init; } = [];
    }

    private sealed class AdMobPublicKey
    {
        [JsonPropertyName("keyId")]
        public JsonElement KeyId { get; init; }

        [JsonPropertyName("base64")]
        public string Base64 { get; init; } = string.Empty;

        public string KeyIdText =>
            KeyId.ValueKind switch
            {
                JsonValueKind.Number => KeyId.GetRawText(),
                JsonValueKind.String => KeyId.GetString() ?? string.Empty,
                _ => string.Empty
            };
    }
}
