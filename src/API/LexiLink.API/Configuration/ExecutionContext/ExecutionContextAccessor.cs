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
                .SingleOrDefault(x => x.Type == "sub")?.Value;

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
}
