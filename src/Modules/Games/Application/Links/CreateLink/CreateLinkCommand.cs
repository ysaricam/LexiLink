using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.CreateLink;

public class CreateLinkCommand : CommandBase<Guid>
{
    public CreateLinkCommand(
        Guid categoryId,
        string value,
        string description,
        bool isActive
    )
    {
        CategoryId = categoryId;
        Value = value;
        Description = description;
        IsActive = isActive;
    }

    public Guid CategoryId { get; }
    public string Value { get; }
    public string Description { get; }
    public bool IsActive { get; }

}