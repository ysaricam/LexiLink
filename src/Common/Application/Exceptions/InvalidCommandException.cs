namespace LexiLink.Common.Application.Exceptions;

public class InvalidCommandException : Exception
{
    public List<string> Errors { get; }
    public IReadOnlyDictionary<string, string[]> ErrorsByProperty { get; }

    public InvalidCommandException(List<string> errors)
    {
        Errors = errors;
        ErrorsByProperty = new Dictionary<string, string[]>
        {
            [""] = errors.ToArray()
        };
    }

    public InvalidCommandException(IReadOnlyDictionary<string, string[]> errorsByProperty)
    {
        ErrorsByProperty = errorsByProperty;
        Errors = errorsByProperty
            .SelectMany(error => error.Value)
            .ToList();
    }
}
