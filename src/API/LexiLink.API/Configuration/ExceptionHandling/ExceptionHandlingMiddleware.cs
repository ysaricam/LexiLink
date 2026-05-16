using System.Text.Json;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ILogger = Serilog.ILogger;

namespace LexiLink.API.Configuration.ExceptionHandling;

public class ExceptionHandlingMiddleware
{
    private const string ProblemJsonContentType = "application/problem+json";

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessRuleValidationException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Business rule violation",
                Detail = ex.Details,
                Type = "https://httpstatuses.com/400",
                Instance = context.Request.Path
            }, extensions =>
            {
                extensions["rule"] = ex.BrokenRule.GetType().Name;
            });
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Detail = $"{ex.EntityName} with id '{ex.Id}' was not found.",
                Type = "https://httpstatuses.com/404",
                Instance = context.Request.Path
            }, extensions =>
            {
                extensions["entityName"] = ex.EntityName;
                extensions["id"] = ex.Id;
            });
        }
        catch (InvalidCommandException ex)
        {
            await WriteProblemDetailsAsync(context, new HttpValidationProblemDetails(ex.ErrorsByProperty)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Type = "https://httpstatuses.com/400",
                Instance = context.Request.Path
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception");
            await WriteProblemDetailsAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",
                Type = "https://httpstatuses.com/500",
                Instance = context.Request.Path
            });
        }
    }

    private static Task WriteProblemDetailsAsync(
        HttpContext context,
        ProblemDetails problemDetails,
        Action<IDictionary<string, object?>>? configureExtensions = null)
    {
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        configureExtensions?.Invoke(problemDetails.Extensions);

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = ProblemJsonContentType;
        return context.Response.WriteAsync(JsonSerializer.Serialize(
            problemDetails,
            problemDetails.GetType(),
            JsonSerializerOptions.Web));
    }
}
