namespace LexiLink.API.Configuration.Health;

public static class DatabaseMigrationScriptMatcher
{
    public static bool IsApplied(string journalScriptName, string expectedRelativePath)
    {
        var normalizedJournalName = NormalizePath(journalScriptName);
        var normalizedExpectedPath = NormalizePath(expectedRelativePath);
        var dotJournalName = ToDotName(normalizedJournalName);
        var dotExpectedPath = ToDotName(normalizedExpectedPath);

        return normalizedJournalName.Equals(normalizedExpectedPath, StringComparison.OrdinalIgnoreCase)
               || normalizedJournalName.EndsWith("/" + normalizedExpectedPath, StringComparison.OrdinalIgnoreCase)
               || dotJournalName.Equals(dotExpectedPath, StringComparison.OrdinalIgnoreCase)
               || dotJournalName.EndsWith("." + dotExpectedPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string value)
    {
        return value
            .Trim()
            .Replace('\\', '/')
            .TrimStart('/');
    }

    private static string ToDotName(string value)
    {
        return value.Replace('/', '.');
    }
}
