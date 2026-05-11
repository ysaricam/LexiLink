using DbUp;
using DbUp.Engine;
using DbUp.ScriptProviders;
using Serilog;
using Serilog.Events;

namespace LexiLink.DatabaseMigrator;

internal static class Program
{
    private static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            if (args.Length < 2)
            {
                Log.Error("Usage: LexiLink.DatabaseMigrator <connectionString> <scriptsDirectory>");
                Log.Error("       <scriptsDirectory> is scanned recursively for *.sql files.");
                return -1;
            }

            var connectionString = args[0];
            var scriptsDirectory = args[1];

            if (!Directory.Exists(scriptsDirectory))
            {
                Log.Error("Scripts directory does not exist: {ScriptsDirectory}", scriptsDirectory);
                return -1;
            }

            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsFromFileSystem(scriptsDirectory, new FileSystemScriptOptions
                {
                    IncludeSubDirectories = true,
                })
                .JournalToPostgresqlTable("public", "MigrationsJournal")
                .LogToConsole()
                .Build();

            var pendingCount = upgrader.GetScriptsToExecute().Count();
            Log.Information("Discovered {Count} pending scripts under {Directory}", pendingCount, scriptsDirectory);

            DatabaseUpgradeResult result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Log.Error(result.Error, "Database upgrade failed");
                return -1;
            }

            Log.Information("Database upgrade succeeded ({Count} scripts applied)", result.Scripts.Count());
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database migrator crashed");
            return -1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
