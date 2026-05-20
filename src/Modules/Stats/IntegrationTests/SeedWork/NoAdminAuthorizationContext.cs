using LexiLink.Common.Application.Admin;

namespace LexiLink.Modules.Stats.IntegrationTests.SeedWork;

/// <summary>
/// Stats integration tests never run admin commands directly; they
/// exercise the projection path. The Players AdminAuditing decorator
/// (introduced in B9) still needs an IAdminAuthorizationContext at
/// activation, so this stub satisfies DI without granting admin
/// authority.
/// </summary>
internal sealed class NoAdminAuthorizationContext : IAdminAuthorizationContext
{
    public bool IsAdmin => false;
    public Guid? AdminUserId => null;
    public Guid RequireAdminUserId() =>
        throw new AdminAuthorizationException("Stats integration tests do not run admin commands.");
    public void EnsureAuthorized() =>
        throw new AdminAuthorizationException("Stats integration tests do not run admin commands.");
}
