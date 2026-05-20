using System.Net;
using System.Text.Json;
using LexiLink.API.Configuration.ExceptionHandling;
using LexiLink.Common.Application.Admin;
using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Domain;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using ILogger = Serilog.ILogger;

namespace LexiLink.API.Tests.ExceptionHandling;

[TestFixture]
public sealed class ExceptionHandlingMiddlewareTests
{
    private ILogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger>();
    }

    [Test]
    public async Task AdminAuthorizationException_Should_Map_To_403_ProblemDetails()
    {
        var sut = new ExceptionHandlingMiddleware(
            next: _ => throw new AdminAuthorizationException("Current request is not running as an authorized admin."),
            logger: _logger);

        var context = CreateContext();

        await sut.Invoke(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        context.Response.ContentType.Should().Be("application/problem+json");
        var body = await ReadBodyAsync(context);
        body.RootElement.GetProperty("status").GetInt32().Should().Be(403);
        body.RootElement.GetProperty("title").GetString().Should().Be("Admin authorization required");
        body.RootElement.GetProperty("detail").GetString()
            .Should().Be("Current request is not running as an authorized admin.");
    }

    [Test]
    public async Task BusinessRuleValidationException_Should_StillMap_To_400_ProblemDetails()
    {
        var sut = new ExceptionHandlingMiddleware(
            next: _ => throw new BusinessRuleValidationException(new AlwaysBrokenRule()),
            logger: _logger);

        var context = CreateContext();

        await sut.Invoke(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/probe";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonDocument.ParseAsync(context.Response.Body);
    }

    private sealed class AlwaysBrokenRule : IBusinessRule
    {
        public bool IsBroken() => true;
        public string Message => "broken";
    }
}
