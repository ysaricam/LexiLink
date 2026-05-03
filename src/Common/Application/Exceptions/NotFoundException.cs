namespace LexiLink.Common.Application.Exceptions;

public class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Id { get; }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.")
    {
        EntityName = entityName;
        Id = id;
    }
}
