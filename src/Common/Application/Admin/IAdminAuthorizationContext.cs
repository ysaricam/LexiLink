namespace LexiLink.Common.Application.Admin;

/// <summary>
/// Cross-cutting context for code paths that must run as an authorized
/// admin. Lives in Common because admin authorization is the same shape
/// across every module today (single `Admin` role, no permission matrix
/// — see ROADMAP non-actions). If permission granularity arrives later,
/// each consumer module gets its own narrower interface and this stays
/// the identity primitive — equivalent to how
/// <see cref="IExecutionContextAccessor"/> already lives in Common.
/// </summary>
public interface IAdminAuthorizationContext
{
    bool IsAdmin { get; }

    /// <summary>
    /// AdminUserId for the current request, or null when the principal
    /// is not an admin (or no execution context is available).
    /// </summary>
    Guid? AdminUserId { get; }

    /// <summary>
    /// Returns the current AdminUserId or throws
    /// <see cref="AdminAuthorizationException"/> when the request is not
    /// running as an authorized admin. Use this in command handlers so
    /// the cross-cutting "must be admin" check is one line and audited
    /// by the exception handler.
    /// </summary>
    Guid RequireAdminUserId();

    /// <summary>
    /// Throws <see cref="AdminAuthorizationException"/> when the request
    /// is not running as an authorized admin. Use when the caller only
    /// needs the assertion and not the id.
    /// </summary>
    void EnsureAuthorized();
}
