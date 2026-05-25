using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Hint.IntegrationTests.SeedWork;

/// <summary>
/// Mutable admin stub mirroring the Energy.IT (B11) template. Default
/// is not-admin; admin tests must call <see cref="LoginAs"/> in
/// [SetUp] before issuing IAdminCommand-decorated commands.
/// </summary>
public sealed class TestAdminAuthorizationContext : IAdminAuthorizationContext
{
    private Guid? _currentAdminId;

    public bool IsAdmin => _currentAdminId is not null;
    public Guid? AdminUserId => _currentAdminId;

    public Guid RequireAdminUserId() =>
        _currentAdminId ?? throw new AdminAuthorizationException(
            "No admin is currently logged in for this Hint integration test.");

    public void EnsureAuthorized()
    {
        if (_currentAdminId is null)
        {
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Hint integration test.");
        }
    }

    public void LoginAs(Guid adminUserId) => _currentAdminId = adminUserId;
    public void Logout() => _currentAdminId = null;
}
