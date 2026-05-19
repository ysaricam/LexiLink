namespace LexiLink.API.Configuration.Bootstrap;

internal sealed class AdministrationBootstrapOptions
{
    public const string SectionName = "Administration:Bootstrap";

    public string[] AdminEmails { get; set; } = [];
}
