using Microsoft.AspNetCore.Mvc;

namespace LexiLink.API.Configuration.ExceptionHandling;

public static class ApiProblemResults
{
    public static IResult NotFound(HttpContext httpContext, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = detail,
            Type = "https://httpstatuses.com/404",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return Results.Problem(problemDetails);
    }
}
