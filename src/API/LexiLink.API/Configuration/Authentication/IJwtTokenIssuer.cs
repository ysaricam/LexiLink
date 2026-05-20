namespace LexiLink.API.Configuration.Authentication;

public interface IJwtTokenIssuer
{
    IssuedToken Issue(Guid playerId);

    /// <summary>
    /// Issues a JWT for an authenticated admin principal. The subject is
    /// the AdminUserId, with `role=Admin` and `admin_id=AdminUserId`
    /// claims attached. Same key/issuer/audience as the player token —
    /// the bearer handler differentiates by inspecting the role claim
    /// and re-verifies admin Active status against Administration.
    /// </summary>
    IssuedToken IssueAdmin(Guid adminUserId);
}
