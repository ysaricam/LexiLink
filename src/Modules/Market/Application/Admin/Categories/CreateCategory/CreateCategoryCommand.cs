using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Admin.Categories.CreateCategory;

public sealed class CreateCategoryCommand : CommandBase<Guid>, IAdminCommand
{
    public string Name { get; }
    public int SortOrder { get; }
    public string? Icon { get; }
    public DateTime? VisibilityStartsAt { get; }
    public DateTime? VisibilityEndsAt { get; }

    public CreateCategoryCommand(
        string name,
        int sortOrder,
        string? icon,
        DateTime? visibilityStartsAt,
        DateTime? visibilityEndsAt)
    {
        Name = name;
        SortOrder = sortOrder;
        Icon = icon;
        VisibilityStartsAt = visibilityStartsAt;
        VisibilityEndsAt = visibilityEndsAt;
    }

    public string AuditTargetType => "Market.Category";
    public string? AuditTargetId => null;
}
