namespace LexiLink.API.Configuration.ExecutionContext;

public class CorrelationMiddleware
{
    public const string CorrelationHeaderKey = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationHeaderKey, out var correlationValue)
            || !Guid.TryParse(correlationValue, out _))
        {
            var generated = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationHeaderKey] = generated;
        }

        return _next(context);
    }
}
