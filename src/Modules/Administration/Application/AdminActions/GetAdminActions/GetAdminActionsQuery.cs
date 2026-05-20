using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.Modules.Administration.Application.AdminActions.GetAdminActions;

public sealed class GetAdminActionsQuery : QueryBase<IReadOnlyList<AdminActionDto>>
{
    public const int MaxLimit = 200;
    public const int DefaultLimit = 50;

    public Guid? AdminUserId { get; }
    public string? TargetType { get; }
    public string? TargetId { get; }
    public int Offset { get; }
    public int Limit { get; }

    public GetAdminActionsQuery(
        Guid? adminUserId = null,
        string? targetType = null,
        string? targetId = null,
        int offset = 0,
        int limit = DefaultLimit)
    {
        AdminUserId = adminUserId;
        TargetType = string.IsNullOrWhiteSpace(targetType) ? null : targetType;
        TargetId = string.IsNullOrWhiteSpace(targetId) ? null : targetId;
        Offset = offset < 0 ? 0 : offset;
        Limit = limit switch
        {
            <= 0 => DefaultLimit,
            > MaxLimit => MaxLimit,
            _ => limit
        };
    }
}
