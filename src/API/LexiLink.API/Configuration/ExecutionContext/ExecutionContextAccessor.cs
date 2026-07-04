using System.Security.Claims;
using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;

namespace LexiLink.API.Configuration.ExecutionContext;

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExecutionContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var subClaim = _httpContextAccessor.HttpContext?.User?.Claims?
                .SingleOrDefault(x => x.Type == "sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.Claims?
                    .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (subClaim != null)
            {
                return Guid.Parse(subClaim);
            }

            throw new ApplicationException("User context is not available");
        }
    }

    public Guid CorrelationId
    {
        get
        {
            if (IsAvailable
                && _httpContextAccessor.HttpContext!.Request.Headers.TryGetValue(
                    CorrelationMiddleware.CorrelationHeaderKey, out var value)
                && Guid.TryParse(value, out var parsed))
            {
                return parsed;
            }

            throw new ApplicationException("Http context and correlation id is not available");
        }
    }

    public bool IsAvailable => _httpContextAccessor.HttpContext != null;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?.User?.Claims
            .Any(c => c.Type == AuthConstants.RoleClaimType && c.Value == AuthConstants.AdminRoleValue)
        ?? false;

    public PlayerAuthSessionMode? PlayerAuthSessionMode
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User?.Claims
                .SingleOrDefault(c => c.Type == AuthConstants.PlayerAuthSessionModeClaimType)?.Value;

            return Enum.TryParse<PlayerAuthSessionMode>(raw, ignoreCase: true, out var mode)
                ? mode
                : null;
        }
    }

    public Guid? AdminUserId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User?.Claims
                .SingleOrDefault(c => c.Type == AuthConstants.AdminUserIdClaimType)?.Value;

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
