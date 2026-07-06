namespace LexiLink.API.Configuration.ExceptionHandling;

internal static class DatabaseExceptionClassifier
{
    private const string PostgresExceptionTypeName = "Npgsql.PostgresException";
    private const string PostgresUniqueViolationSqlState = "23505";

    public static bool IsPostgresUniqueViolation(Exception exception, string constraintName)
    {
        var current = exception;
        while (current is not null)
        {
            if (current.GetType().FullName == PostgresExceptionTypeName &&
                GetStringProperty(current, "SqlState") == PostgresUniqueViolationSqlState &&
                GetStringProperty(current, "ConstraintName") == constraintName)
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }

    private static string? GetStringProperty(Exception exception, string propertyName) =>
        exception.GetType().GetProperty(propertyName)?.GetValue(exception) as string;
}
