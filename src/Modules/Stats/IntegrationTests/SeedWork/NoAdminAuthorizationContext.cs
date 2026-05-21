using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Stats.IntegrationTests.SeedWork;

/// <summary>
/// Stats integration tests boot Games + Players to drive the projection
/// path. B7/B9/B10 turned several producer-side commands into
/// IAdminCommand, so the cross-module activation chain demands a
/// logged-in admin even when Stats itself is the subject under test.
/// This stub reports an always-logged-in synthetic admin so the
/// AdminAuditing decorator's <c>RequireAdminUserId()</c> succeeds;
/// the produced audit rows are clean by-products of the test
/// arrangement (and are cleared between tests).
/// </summary>
internal sealed class NoAdminAuthorizationContext : IAdminAuthorizationContext
{
    public static readonly Guid SyntheticAdminId =
        Guid.Parse("88888888-0000-0000-0000-000000000001");

    public bool IsAdmin => true;
    public Guid? AdminUserId => SyntheticAdminId;
    public Guid RequireAdminUserId() => SyntheticAdminId;
    public void EnsureAuthorized() { }
}
