namespace LexiLink.Common.Application.Admin;

/// <summary>
/// Thrown when a command or query that requires admin authorization is
/// invoked without an admin principal in scope. The API exception
/// handling middleware maps this to a 403 ProblemDetails response.
/// </summary>
public sealed class AdminAuthorizationException : Exception
{
    public AdminAuthorizationException(string message) : base(message)
    {
    }
}
