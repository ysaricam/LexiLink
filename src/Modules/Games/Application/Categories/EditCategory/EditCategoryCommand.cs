using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.EditCategory;

public class EditCategoryCommand : CommandBase
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
}
