using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Quests.IntegrationTests.SeedWork;

/// <summary>
/// Mutable admin authorization stub for Quests integration tests.
/// Defaults to "not admin" so non-admin tests (existing 5) behave as
/// before. Admin IT tests call <see cref="LoginAs"/> in [SetUp] to
/// pretend an admin is making the request; <see cref="Logout"/> in
/// [TearDown] restores the default. Single instance is shared across
/// the test container scope, so toggling it before resolving a
/// command handler is enough.
/// </summary>
public sealed class TestAdminAuthorizationContext : IAdminAuthorizationContext
{
    private Guid? _currentAdminId;

    public bool IsAdmin => _currentAdminId is not null;
    public Guid? AdminUserId => _currentAdminId;

    public Guid RequireAdminUserId() =>
        _currentAdminId ?? throw new AdminAuthorizationException(
            "No admin is currently logged in for this Quests integration test.");

    public void EnsureAuthorized()
    {
        if (_currentAdminId is null)
        {
            throw new AdminAuthorizationException(
                "No admin is currently logged in for this Quests integration test.");
        }
    }

    public void LoginAs(Guid adminUserId) => _currentAdminId = adminUserId;
    public void Logout() => _currentAdminId = null;
}
