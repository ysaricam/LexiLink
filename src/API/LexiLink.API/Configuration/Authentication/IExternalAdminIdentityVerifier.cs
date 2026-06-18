using System.Security.Cryptography;
using System.Text;

namespace LexiLink.API.Configuration.Authentication;

/// <summary>
/// Verifies an admin-side external identity claim before
/// <c>/auth/admin/token</c> issues a first-party JWT. Mirrors
/// <see cref="IExternalIdentityVerifier"/> for player tokens but is keyed
/// off the admin's email rather than an Apple/Google provider id —
/// real provider integration (SSO) lands once an admin frontend exists.
/// </summary>
public interface IExternalAdminIdentityVerifier
{
    Task<bool> VerifyAsync(string email, string externalToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Development-only verifier: accepts the literal token
/// <c>dev:admin:{email}</c>. Disabled outside Production-mode is enforced
/// by <see cref="LexiLinkAuthOptionsValidator"/>.
/// </summary>
public sealed class DevelopmentExternalAdminIdentityVerifier : IExternalAdminIdentityVerifier
{
    public Task<bool> VerifyAsync(string email, string externalToken, CancellationToken cancellationToken = default)
    {
        var expected = $"dev:admin:{email}";
        return Task.FromResult(string.Equals(externalToken, expected, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class DisabledExternalAdminIdentityVerifier : IExternalAdminIdentityVerifier
{
    public Task<bool> VerifyAsync(string email, string externalToken, CancellationToken cancellationToken = default)
        => Task.FromResult(false);
}

/// <summary>
/// Production-capable verifier for the first browser admin panel: accepts a
/// strong operator-owned shared token, then lets <c>/auth/admin/token</c>
/// require that the supplied email belongs to an active AdminUser.
/// </summary>
public sealed class SharedSecretExternalAdminIdentityVerifier : IExternalAdminIdentityVerifier
{
    private readonly string _sharedSecret;

    public SharedSecretExternalAdminIdentityVerifier(string sharedSecret)
    {
        if (string.IsNullOrWhiteSpace(sharedSecret))
        {
            throw new ArgumentException("Shared secret is required.", nameof(sharedSecret));
        }

        _sharedSecret = sharedSecret;
    }

    public Task<bool> VerifyAsync(string email, string externalToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalToken))
        {
            return Task.FromResult(false);
        }

        var expected = Encoding.UTF8.GetBytes(_sharedSecret);
        var actual = Encoding.UTF8.GetBytes(externalToken);

        if (expected.Length != actual.Length)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(CryptographicOperations.FixedTimeEquals(expected, actual));
    }
}
