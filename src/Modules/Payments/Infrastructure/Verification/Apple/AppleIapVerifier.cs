using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;
using Microsoft.Extensions.Options;

namespace LexiLink.Modules.Payments.Infrastructure.Verification.Apple;

internal sealed class AppleIapVerifier : IAppleIapVerifier
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppleIapOptions _options;
    private readonly HttpClient _httpClient;

    internal AppleIapVerifier(IOptions<AppleIapOptions> options)
    {
        _options = options.Value;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(GetBaseUrl(_options.Environment))
        };
    }

    public async Task<StorePurchaseVerificationResult> VerifyAsync(
        AppleIapVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured(_options))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple IAP verification is not configured.");
        }

        if (string.IsNullOrWhiteSpace(request.TransactionId))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple transaction id is required.");
        }

        AppStoreTransactionResponse serverResponse;
        try
        {
            serverResponse = await GetTransactionAsync(request.TransactionId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (IsVerificationInfrastructureException(exception))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                $"Apple transaction verification could not be completed: {exception.Message}");
        }
        if (serverResponse.Failure is not null)
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                serverResponse.Failure);
        }

        if (string.IsNullOrWhiteSpace(serverResponse.SignedTransactionInfo))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple transaction response did not include signed transaction info.");
        }

        var decodedTransaction = DecodeAndVerifyTransactionJws(serverResponse.SignedTransactionInfo);
        if (decodedTransaction.Failure is not null || decodedTransaction.Payload is null)
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                _options.Environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                decodedTransaction.Failure ?? "Apple transaction JWS could not be decoded.");
        }

        var payload = decodedTransaction.Payload;
        var environment = ParseEnvironment(payload.Environment) ?? _options.Environment;

        if (!string.Equals(payload.BundleId, _options.BundleId, StringComparison.Ordinal))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple transaction bundle id does not match configured bundle id.");
        }

        if (!string.Equals(payload.TransactionId, request.TransactionId, StringComparison.Ordinal))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                environment,
                request.StoreProductId,
                request.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple transaction id does not match the verified transaction.");
        }

        if (!string.Equals(payload.ProductId, request.StoreProductId, StringComparison.Ordinal))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                environment,
                payload.ProductId ?? request.StoreProductId,
                payload.TransactionId,
                purchaseToken: null,
                StorePurchaseState.ProductMismatch,
                "Apple transaction product id does not match requested product id.");
        }

        if (environment != _options.Environment)
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                environment,
                payload.ProductId ?? request.StoreProductId,
                payload.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Invalid,
                "Apple transaction environment does not match configured environment.");
        }

        if (request.AppAccountToken is not null
            && payload.AppAccountToken is not null
            && !string.Equals(payload.AppAccountToken, request.AppAccountToken, StringComparison.OrdinalIgnoreCase))
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                environment,
                payload.ProductId ?? request.StoreProductId,
                payload.TransactionId,
                purchaseToken: null,
                StorePurchaseState.AccountMismatch,
                "Apple transaction app account token does not match the authenticated player.");
        }

        if (payload.RevocationDate is not null)
        {
            return StorePurchaseVerificationResult.Failed(
                PaymentPlatform.Apple,
                environment,
                payload.ProductId ?? request.StoreProductId,
                payload.TransactionId,
                purchaseToken: null,
                StorePurchaseState.Revoked,
                "Apple transaction has been revoked.");
        }

        return StorePurchaseVerificationResult.Verified(
            PaymentPlatform.Apple,
            environment,
            payload.ProductId ?? request.StoreProductId,
            payload.TransactionId,
            purchaseToken: null,
            orderId: payload.WebOrderLineItemId,
            accountToken: payload.AppAccountToken,
            StorePurchasePostProcessingAction.AppleClientFinishTransaction,
            ToDateTime(payload.PurchaseDate));
    }

    private async Task<AppStoreTransactionResponse> GetTransactionAsync(
        string transactionId,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/inApps/v1/transactions/{Uri.EscapeDataString(transactionId)}");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateAuthorizationToken());

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = TryDeserialize<AppStoreErrorResponse>(responseJson);
            var message = error is not null
                ? $"Apple transaction verification failed: {error.ErrorCode} {error.ErrorMessage}".TrimEnd()
                : $"Apple transaction verification failed with HTTP {(int)response.StatusCode}.";

            return new AppStoreTransactionResponse(null, message);
        }

        var payload = TryDeserialize<AppStoreTransactionLookupResponse>(responseJson);
        return payload is null
            ? new AppStoreTransactionResponse(null, "Apple transaction response could not be parsed.")
            : new AppStoreTransactionResponse(payload.SignedTransactionInfo, null);
    }

    private string CreateAuthorizationToken()
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var header = JsonSerializer.Serialize(new AppStoreJwtHeader("ES256", _options.KeyId, "JWT"), JsonOptions);
        var payload = JsonSerializer.Serialize(new AppStoreJwtPayload(
            _options.IssuerId,
            issuedAt.ToUnixTimeSeconds(),
            issuedAt.AddMinutes(5).ToUnixTimeSeconds(),
            "appstoreconnect-v1",
            _options.BundleId), JsonOptions);

        var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signingInput = $"{encodedHeader}.{encodedPayload}";

        using var key = ECDsa.Create();
        key.ImportFromPem(NormalizePrivateKey(_options.PrivateKey));
        var signature = key.SignData(
            Encoding.ASCII.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        return $"{signingInput}.{Base64UrlEncode(signature)}";
    }

    private static DecodedTransactionResult DecodeAndVerifyTransactionJws(string signedTransactionJws)
    {
        var parts = signedTransactionJws.Split('.');
        if (parts.Length != 3)
        {
            return new DecodedTransactionResult(null, "Apple transaction JWS has an invalid format.");
        }

        try
        {
            var header = TryDeserialize<AppStoreJwsHeader>(Encoding.UTF8.GetString(Base64UrlDecode(parts[0])));
            if (header?.CertificateChain is null || header.CertificateChain.Length == 0)
            {
                return new DecodedTransactionResult(null, "Apple transaction JWS did not include a signing certificate.");
            }

            using var certificate = X509CertificateLoader.LoadCertificate(
                Convert.FromBase64String(header.CertificateChain[0]));
            using var publicKey = certificate.GetECDsaPublicKey();
            if (publicKey is null)
            {
                return new DecodedTransactionResult(null, "Apple transaction JWS signing certificate is not ECDSA.");
            }

            var isSignatureValid = publicKey.VerifyData(
                Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}"),
                Base64UrlDecode(parts[2]),
                HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

            if (!isSignatureValid)
            {
                return new DecodedTransactionResult(null, "Apple transaction JWS signature is invalid.");
            }

            var payload = TryDeserialize<AppStoreTransactionPayload>(Encoding.UTF8.GetString(Base64UrlDecode(parts[1])));
            return payload is null
                ? new DecodedTransactionResult(null, "Apple transaction JWS payload could not be parsed.")
                : new DecodedTransactionResult(payload, null);
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException or ArgumentException)
        {
            return new DecodedTransactionResult(null, "Apple transaction JWS could not be verified.");
        }
    }

    private static bool IsConfigured(AppleIapOptions options) =>
        !string.IsNullOrWhiteSpace(options.BundleId)
        && !string.IsNullOrWhiteSpace(options.IssuerId)
        && !string.IsNullOrWhiteSpace(options.KeyId)
        && !string.IsNullOrWhiteSpace(options.PrivateKey);

    private static string GetBaseUrl(PaymentEnvironment environment) =>
        environment == PaymentEnvironment.Production
            ? "https://api.storekit.itunes.apple.com"
            : "https://api.storekit-sandbox.itunes.apple.com";

    private static string NormalizePrivateKey(string privateKey) =>
        privateKey.Replace("\\n", "\n", StringComparison.Ordinal);

    private static PaymentEnvironment? ParseEnvironment(string? environment) =>
        environment?.ToLowerInvariant() switch
        {
            "production" => PaymentEnvironment.Production,
            "sandbox" => PaymentEnvironment.Sandbox,
            _ => null
        };

    private static DateTime? ToDateTime(long? unixMilliseconds) =>
        unixMilliseconds is null
            ? null
            : DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds.Value).UtcDateTime;

    private static bool IsVerificationInfrastructureException(Exception exception) =>
        exception is HttpRequestException
            or CryptographicException
            or FormatException
            or ArgumentException
            or JsonException;

    private static T? TryDeserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
        catch (FormatException)
        {
            return default;
        }
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal);

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value
            .Replace("-", "+", StringComparison.Ordinal)
            .Replace("_", "/", StringComparison.Ordinal);

        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
        return Convert.FromBase64String(base64);
    }

    private sealed record AppStoreTransactionResponse(string? SignedTransactionInfo, string? Failure);

    private sealed record DecodedTransactionResult(AppStoreTransactionPayload? Payload, string? Failure);

    private sealed record AppStoreTransactionLookupResponse(
        [property: JsonPropertyName("signedTransactionInfo")] string? SignedTransactionInfo);

    private sealed record AppStoreErrorResponse(
        [property: JsonPropertyName("errorCode")] long? ErrorCode,
        [property: JsonPropertyName("errorMessage")] string? ErrorMessage);

    private sealed record AppStoreJwtHeader(
        [property: JsonPropertyName("alg")] string Algorithm,
        [property: JsonPropertyName("kid")] string KeyId,
        [property: JsonPropertyName("typ")] string Type);

    private sealed record AppStoreJwtPayload(
        [property: JsonPropertyName("iss")] string IssuerId,
        [property: JsonPropertyName("iat")] long IssuedAt,
        [property: JsonPropertyName("exp")] long ExpiresAt,
        [property: JsonPropertyName("aud")] string Audience,
        [property: JsonPropertyName("bid")] string BundleId);

    private sealed record AppStoreJwsHeader(
        [property: JsonPropertyName("x5c")] string[]? CertificateChain);

    private sealed record AppStoreTransactionPayload(
        [property: JsonPropertyName("transactionId")] string? TransactionId,
        [property: JsonPropertyName("originalTransactionId")] string? OriginalTransactionId,
        [property: JsonPropertyName("webOrderLineItemId")] string? WebOrderLineItemId,
        [property: JsonPropertyName("bundleId")] string? BundleId,
        [property: JsonPropertyName("productId")] string? ProductId,
        [property: JsonPropertyName("purchaseDate")] long? PurchaseDate,
        [property: JsonPropertyName("revocationDate")] long? RevocationDate,
        [property: JsonPropertyName("environment")] string? Environment,
        [property: JsonPropertyName("appAccountToken")] string? AppAccountToken);
}
