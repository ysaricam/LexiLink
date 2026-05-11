using System.Text.Json;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Domain;
using Serilog;
using ILogger = Serilog.ILogger;

namespace LexiLink.API.Configuration.ExceptionHandling;

public class ExceptionHandlingMiddleware
{
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
            await WriteJsonAsync(context, StatusCodes.Status400BadRequest, new
            {
                status = StatusCodes.Status400BadRequest,
                title = "Business rule violation",
                detail = ex.Details,
                rule = ex.BrokenRule.GetType().Name
            });
        }
        catch (NotFoundException ex)
        {
            await WriteJsonAsync(context, StatusCodes.Status404NotFound, new
            {
                status = StatusCodes.Status404NotFound,
                title = "Not Found",
                entityName = ex.EntityName,
                id = ex.Id
            });
        }
        catch (InvalidCommandException ex)
        {
            await WriteJsonAsync(context, StatusCodes.Status422UnprocessableEntity, new
            {
                status = StatusCodes.Status422UnprocessableEntity,
                title = "Invalid command",
                errors = ex.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception");
            await WriteJsonAsync(context, StatusCodes.Status500InternalServerError, new
            {
                status = StatusCodes.Status500InternalServerError,
                title = "Internal server error"
            });
        }
    }

    private static Task WriteJsonAsync(HttpContext context, int statusCode, object payload)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
