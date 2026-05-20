using LexiLink.Common.Application;
using LexiLink.Common.Application.Admin;

namespace LexiLink.API.CrossModule;

/// <summary>
/// API-host implementation of <see cref="IAdminAuthorizationContext"/>.
/// Reads the IsAdmin / AdminUserId values that
/// <see cref="LexiLink.API.Configuration.Authentication.LexiLinkBearerAuthenticationHandler"/>
/// stamped onto the principal, so module Application code never touches
/// HttpContext directly.
/// </summary>
public sealed class AdminAuthorizationContext : IAdminAuthorizationContext
{
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public AdminAuthorizationContext(IExecutionContextAccessor executionContextAccessor)
    {
        _executionContextAccessor = executionContextAccessor;
    }

    public bool IsAdmin =>
        _executionContextAccessor.IsAvailable
        && _executionContextAccessor.IsAdmin
        && _executionContextAccessor.AdminUserId is not null;

    public Guid? AdminUserId =>
        _executionContextAccessor.IsAvailable && _executionContextAccessor.IsAdmin
            ? _executionContextAccessor.AdminUserId
            : null;

    public Guid RequireAdminUserId()
    {
        EnsureAuthorized();
        return _executionContextAccessor.AdminUserId!.Value;
    }

    public void EnsureAuthorized()
    {
        if (!IsAdmin)
        {
            throw new AdminAuthorizationException(
                "Current request is not running as an authorized admin.");
        }
    }
}
