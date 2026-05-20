namespace LexiLink.API.Configuration.Authentication;

public static class AuthConstants
{
    public const string DevelopmentBearerScheme = "LexiLinkDevelopmentBearer";
    public const string Scheme = DevelopmentBearerScheme;
    public const string AuthenticatedPlayerPolicy = "AuthenticatedPlayer";
    public const string AuthenticatedAdminPolicy = "AuthenticatedAdmin";

    public const string RoleClaimType = "role";
    public const string AdminRoleValue = "Admin";

    /// <summary>
    /// Claim carrying the AdminUserId for an authenticated admin principal.
    /// Stored separately from `sub` so an admin token can also identify a
    /// principal via the regular `sub`/UserId pipeline without collisions.
    /// </summary>
    public const string AdminUserIdClaimType = "admin_id";
}
