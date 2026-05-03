using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.CreateCategory;

public class CreateCategoryCommand : CommandBase<Guid>
{
    public string Name { get; }
    public string Description { get; }

    public CreateCategoryCommand(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
