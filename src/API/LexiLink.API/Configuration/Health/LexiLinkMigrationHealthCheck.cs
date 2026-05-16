using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace LexiLink.API.Configuration.Health;

public sealed class LexiLinkMigrationHealthCheck(
    string connectionString,
    string scriptsDirectory) : IHealthCheck
{
    private const int MaxReportedMissingScripts = 20;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expectedScripts = GetExpectedScripts();
            if (expectedScripts.Count == 0)
            {
                return HealthCheckResult.Unhealthy(
                    "No DbUp scripts were found in the API artifact.",
                    data: new Dictionary<string, object>
                    {
                        ["scriptsDirectory"] = scriptsDirectory
                    });
            }

            var appliedScripts = await GetAppliedScriptsAsync(cancellationToken);
            var missingScripts = expectedScripts
                .Where(expectedScript => appliedScripts.All(appliedScript =>
                    !DatabaseMigrationScriptMatcher.IsApplied(appliedScript, expectedScript)))
                .ToArray();

            if (missingScripts.Length > 0)
            {
                return HealthCheckResult.Unhealthy(
                    $"{missingScripts.Length} DbUp script(s) have not been applied.",
                    data: new Dictionary<string, object>
                    {
                        ["missingScripts"] = missingScripts.Take(MaxReportedMissingScripts).ToArray(),
                        ["missingScriptsCount"] = missingScripts.Length,
                        ["expectedScriptsCount"] = expectedScripts.Count,
                        ["appliedScriptsCount"] = appliedScripts.Count
                    });
            }

            return HealthCheckResult.Healthy(
                $"{expectedScripts.Count} DbUp script(s) are applied.",
                data: new Dictionary<string, object>
                {
                    ["expectedScriptsCount"] = expectedScripts.Count,
                    ["appliedScriptsCount"] = appliedScripts.Count
                });
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("DbUp migration journal could not be validated.", exception);
        }
    }

    private IReadOnlyCollection<string> GetExpectedScripts()
    {
        if (!Directory.Exists(scriptsDirectory))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(scriptsDirectory, "*.sql", SearchOption.AllDirectories)
            .Select(scriptPath => Path.GetRelativePath(scriptsDirectory, scriptPath))
            .Select(scriptPath => scriptPath.Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<string>> GetAppliedScriptsAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var (journalTableName, scriptNameColumnName) = await GetJournalNamesAsync(connection, cancellationToken);
        var quotedTableName = QuoteIdentifier(journalTableName);
        var quotedColumnName = QuoteIdentifier(scriptNameColumnName);

        await using var command = new NpgsqlCommand(
            $"SELECT {quotedColumnName} FROM public.{quotedTableName};",
            connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var scripts = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            scripts.Add(reader.GetString(0));
        }

        return scripts;
    }

    private static async Task<(string TableName, string ScriptNameColumnName)> GetJournalNamesAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT table_name, column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND lower(table_name) = lower('MigrationsJournal')
              AND lower(column_name) = lower('ScriptName')
            LIMIT 1;
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "DbUp journal table public.MigrationsJournal or its ScriptName column was not found.");
        }

        return (reader.GetString(0), reader.GetString(1));
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
