using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Admin.Categories.DeactivateCategory;

public sealed class DeactivateCategoryCommand : CommandBase, IAdminCommand
{
    public Guid CategoryId { get; }

    public DeactivateCategoryCommand(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public string AuditTargetType => "Market.Category";
    public string? AuditTargetId => CategoryId.ToString();
}
