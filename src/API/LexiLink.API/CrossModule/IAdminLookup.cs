namespace LexiLink.API.CrossModule;

/// <summary>
/// API-host facing query gateway over Administration's admin-user state.
/// Used by the authentication handler (per-request) and the
/// `/auth/admin/token` endpoint (per token issuance) to verify that the
/// principal is a currently-Active admin. Modeled after IEnergyGuard:
/// interface lives where the consumer reads it, adapter wires through
/// the Administration module facade.
/// </summary>
public interface IAdminLookup
{
    Task<AdminLookupResult?> FindActiveByIdAsync(Guid adminUserId, CancellationToken cancellationToken = default);

    Task<AdminLookupResult?> FindActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public sealed record AdminLookupResult(Guid AdminUserId, string Email, string Role);
