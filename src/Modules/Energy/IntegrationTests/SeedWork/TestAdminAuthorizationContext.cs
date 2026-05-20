using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Energy.IntegrationTests.SeedWork;

/// <summary>
/// Same mutable admin stub pattern as Quests.IT (B7). Default = not
/// admin so existing 4 IT tests are unaffected; admin tests call
/// <see cref="LoginAs"/> in [SetUp].
/// </summary>
public sealed class TestAdminAuthorizationContext : IAdminAuthorizationContext
{
    private Guid? _currentAdminId;

    public bool IsAdmin => _currentAdminId is not null;
    public Guid? AdminUserId => _currentAdminId;

    public Guid RequireAdminUserId() =>
        _currentAdminId ?? throw new AdminAuthorizationException(
            "No admin is currently logged in for this Energy integration test.");

    public void EnsureAuthorized()
    {
        if (_currentAdminId is null)
        {
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Energy integration test.");
        }
    }

    public void LoginAs(Guid adminUserId) => _currentAdminId = adminUserId;
    public void Logout() => _currentAdminId = null;
}
