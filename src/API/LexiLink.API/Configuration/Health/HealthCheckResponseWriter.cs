using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LexiLink.API.Configuration.Health;

public static class HealthCheckResponseWriter
{
    public static Task WriteJsonAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    durationMilliseconds = entry.Value.Duration.TotalMilliseconds
                })
        });
    }
}
