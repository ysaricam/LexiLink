using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.EditCategory;

public class EditCategoryCommand : CommandBase, IAdminCommand
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public string Description { get; }
    public string Language { get; }

    public EditCategoryCommand(Guid categoryId, string name, string description, string language = "tr-TR")
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Language = language;
    }

    public string AuditTargetType => "Games.Category";
    public string? AuditTargetId => CategoryId.ToString();
}
