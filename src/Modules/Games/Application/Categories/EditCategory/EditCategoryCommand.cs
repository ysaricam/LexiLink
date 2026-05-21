using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.EditCategory;

public class EditCategoryCommand : CommandBase, IAdminCommand
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public string Description { get; }

    public EditCategoryCommand(Guid categoryId, string name, string description)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
    }

    public string AuditTargetType => "Games.Category";
    public string? AuditTargetId => CategoryId.ToString();
}
