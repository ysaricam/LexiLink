using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Npgsql;

namespace LexiLink.Tools.CategoryImporter;

internal static class Program
{
    private const string DefaultLanguage = "tr-TR";
    private static readonly Regex LanguagePattern = new("^[a-z]{2}-[A-Z]{2}$", RegexOptions.Compiled);

    private static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: LexiLink.Tools.CategoryImporter <connectionString> <categoryJsonPath>");
            return 1;
        }

        var connectionString = args[0];
        var categoryJsonPath = args[1];

        if (!File.Exists(categoryJsonPath))
        {
            Console.Error.WriteLine($"Category JSON file does not exist: {categoryJsonPath}");
            return 1;
        }

        var document = await ReadDocumentAsync(categoryJsonPath);
        var validationError = Validate(document);
        if (validationError is not null)
        {
            Console.Error.WriteLine(validationError);
            return 1;
        }

        var language = NormalizeLanguage(document.Category.Language);
        var categoryId = StableGuid($"category:{language}:{document.Category.Name}");
        var linkIdsByValue = document.Links.ToDictionary(
            link => link.Value,
            link => StableGuid($"category:{language}:{document.Category.Name}:link:{link.Value}"),
            StringComparer.Ordinal);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await UpsertCategoryAsync(connection, transaction, categoryId, document.Category, language);

        foreach (var link in document.Links)
        {
            await UpsertLinkAsync(
                connection,
                transaction,
                linkIdsByValue[link.Value],
                categoryId,
                link);
        }

        await DeleteImportedOutgoingLinksAsync(connection, transaction, linkIdsByValue.Values);

        foreach (var edge in document.Edges)
        {
            await InsertEdgeAsync(
                connection,
                transaction,
                linkIdsByValue[edge.From],
                linkIdsByValue[edge.To]);
        }

        await transaction.CommitAsync();

        Console.WriteLine(
            $"Imported category '{document.Category.Name}' [{language}] ({document.Links.Count} links, {document.Edges.Count} edges).");
        Console.WriteLine($"CategoryId: {categoryId}");
        return 0;
    }

    private static async Task<CategoryDocument> ReadDocumentAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var document = await JsonSerializer.DeserializeAsync<CategoryDocument>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

        return document ?? throw new InvalidOperationException("Category JSON is empty.");
    }

    private static string? Validate(CategoryDocument document)
    {
        if (document.Category.Name.Length == 0)
            return "Category name must not be empty.";

        var language = NormalizeLanguage(document.Category.Language);
        if (!LanguagePattern.IsMatch(language))
            return "Category language must be in BCP 47 short form (e.g. 'tr-TR', 'en-US').";

        var duplicateLink = document.Links
            .GroupBy(link => link.Value, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateLink is not null)
            return $"Duplicate link value: {duplicateLink.Key}";

        var linkValues = document.Links
            .Select(link => link.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var edge in document.Edges)
        {
            if (!linkValues.Contains(edge.From))
                return $"Edge source is missing from links: {edge.From}";

            if (!linkValues.Contains(edge.To))
                return $"Edge target is missing from links: {edge.To}";
        }

        var duplicateEdge = document.Edges
            .GroupBy(edge => $"{edge.From}\t{edge.To}", StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateEdge is not null)
            return $"Duplicate edge: {duplicateEdge.Key}";

        return null;
    }

    private static async Task UpsertCategoryAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid categoryId,
        CategoryInfo category,
        string language)
    {
        const string sql = """
            INSERT INTO "games"."Categories" ("Id", "Name", "Description", "Language")
            VALUES (@Id, @Name, @Description, @Language)
            ON CONFLICT ("Id") DO UPDATE
            SET "Name" = EXCLUDED."Name",
                "Description" = EXCLUDED."Description",
                "Language" = EXCLUDED."Language";
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("Id", categoryId);
        command.Parameters.AddWithValue("Name", category.Name);
        command.Parameters.AddWithValue("Description", category.Description);
        command.Parameters.AddWithValue("Language", language);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task UpsertLinkAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid linkId,
        Guid categoryId,
        LinkInfo link)
    {
        const string sql = """
            INSERT INTO "games"."Links" ("Id", "Value", "Description", "IsActive", "CategoryId")
            VALUES (@Id, @Value, @Description, true, @CategoryId)
            ON CONFLICT ("Id") DO UPDATE
            SET "Value" = EXCLUDED."Value",
                "Description" = EXCLUDED."Description",
                "IsActive" = true,
                "CategoryId" = EXCLUDED."CategoryId";
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("Id", linkId);
        command.Parameters.AddWithValue("Value", link.Value);
        command.Parameters.AddWithValue("Description", link.Description);
        command.Parameters.AddWithValue("CategoryId", categoryId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DeleteImportedOutgoingLinksAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        IEnumerable<Guid> linkIds)
    {
        const string sql = """
            DELETE FROM "games"."LinkOutgoingLinks"
            WHERE "LinkId" = ANY(@LinkIds);
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("LinkIds", linkIds.ToArray());
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertEdgeAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid sourceLinkId,
        Guid targetLinkId)
    {
        const string sql = """
            INSERT INTO "games"."LinkOutgoingLinks" ("LinkId", "OutgoingLinkId")
            VALUES (@LinkId, @OutgoingLinkId)
            ON CONFLICT ("LinkId", "OutgoingLinkId") DO NOTHING;
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("LinkId", sourceLinkId);
        command.Parameters.AddWithValue("OutgoingLinkId", targetLinkId);
        await command.ExecuteNonQueryAsync();
    }

    private static Guid StableGuid(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x30);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes);
    }

    private static string NormalizeLanguage(string? language) =>
        string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language;
}

internal sealed record CategoryDocument(
    [property: JsonPropertyName("$schema")] string Schema,
    CategoryInfo Category,
    IReadOnlyList<LinkInfo> Links,
    IReadOnlyList<EdgeInfo> Edges,
    MetadataInfo? Metadata);

internal sealed record CategoryInfo(string Name, string Description, string? Language);

internal sealed record LinkInfo(
    string Value,
    string Description,
    string WikipediaUrl,
    int DepthFromAnchor);

internal sealed record EdgeInfo(string From, string To);

internal sealed record MetadataInfo(
    string Source,
    bool Symmetrized,
    int NodeCount,
    int EdgeCount,
    DateTimeOffset GeneratedAt,
    string GeneratorVersion);
