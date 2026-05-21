using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Games.IntegrationTests.SeedWork;

/// <summary>
/// Mutable admin auth stub — same pattern as Quests/Energy/Players IT
/// (B7-B9). Default = not admin so the existing IT tests behave as
/// before. Admin tests call LoginAs in [SetUp].
/// </summary>
public sealed class TestAdminAuthorizationContext : IAdminAuthorizationContext
{
    private Guid? _currentAdminId;

    public bool IsAdmin => _currentAdminId is not null;
    public Guid? AdminUserId => _currentAdminId;

    public Guid RequireAdminUserId() =>
        _currentAdminId ?? throw new AdminAuthorizationException(
            "No admin is currently logged in for this Games integration test.");

    public void EnsureAuthorized()
    {
        if (_currentAdminId is null)
        {
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Games integration test.");
        }
    }

    public void LoginAs(Guid adminUserId) => _currentAdminId = adminUserId;
    public void Logout() => _currentAdminId = null;
}
