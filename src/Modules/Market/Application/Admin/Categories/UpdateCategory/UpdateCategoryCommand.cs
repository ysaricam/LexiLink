using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Admin.Categories.UpdateCategory;

public sealed class UpdateCategoryCommand : CommandBase, IAdminCommand
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public int SortOrder { get; }
    public string? Icon { get; }
    public DateTime? VisibilityStartsAt { get; }
    public DateTime? VisibilityEndsAt { get; }

    public UpdateCategoryCommand(
        Guid categoryId,
        string name,
        int sortOrder,
        string? icon,
        DateTime? visibilityStartsAt,
        DateTime? visibilityEndsAt)
    {
        CategoryId = categoryId;
        Name = name;
        SortOrder = sortOrder;
        Icon = icon;
        VisibilityStartsAt = visibilityStartsAt;
        VisibilityEndsAt = visibilityEndsAt;
    }

    public string AuditTargetType => "Market.Category";
    public string? AuditTargetId => CategoryId.ToString();
}
