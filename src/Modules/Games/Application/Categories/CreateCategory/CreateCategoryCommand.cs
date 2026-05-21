using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.CreateCategory;

public class CreateCategoryCommand : CommandBase<Guid>, IAdminCommand
{
    public string Name { get; }
    public string Description { get; }

    public CreateCategoryCommand(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string AuditTargetType => "Games.Category";
    // Id is allocated by the handler; PayloadJson carries the new id.
    public string? AuditTargetId => null;
}
