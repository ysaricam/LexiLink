namespace LexiLink.Common.Application;

public interface IExecutionContextAccessor
{
    Guid UserId { get; }

    Guid CorrelationId { get; }

    bool IsAvailable { get; }

    /// <summary>
    /// True when the authenticated principal carries the Administration
    /// role claim. Player and anonymous requests return false.
    /// </summary>
    bool IsAdmin { get; }

    PlayerAuthSessionMode? PlayerAuthSessionMode { get; }

    /// <summary>
    /// AdminUserId of the authenticated admin principal, or null for
    /// player/anonymous requests. Resolved from the admin claim populated
    /// by the API host's authentication handler — Administration looks
    /// the AdminUser up by id and verifies Active status before the claim
    /// is issued.
    /// </summary>
    Guid? AdminUserId { get; }
}
