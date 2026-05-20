using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Players.IntegrationTests.SeedWork;

/// <summary>
/// Mutable admin auth stub — same pattern as Quests/Energy IT
/// (B7/B8). Default = not admin; admin tests call LoginAs in [SetUp].
/// </summary>
public sealed class TestAdminAuthorizationContext : IAdminAuthorizationContext
{
    private Guid? _currentAdminId;

    public bool IsAdmin => _currentAdminId is not null;
    public Guid? AdminUserId => _currentAdminId;

    public Guid RequireAdminUserId() =>
        _currentAdminId ?? throw new AdminAuthorizationException(
            "No admin is currently logged in for this Players integration test.");

    public void EnsureAuthorized()
    {
        if (_currentAdminId is null)
        {
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Players integration test.");
        }
    }

    public void LoginAs(Guid adminUserId) => _currentAdminId = adminUserId;
    public void Logout() => _currentAdminId = null;
}
