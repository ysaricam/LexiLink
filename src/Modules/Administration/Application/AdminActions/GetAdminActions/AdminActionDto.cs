namespace LexiLink.Modules.Administration.Application.AdminActions.GetAdminActions;

public sealed record AdminActionDto(
    Guid Id,
    DateTime OccurredOn,
    Guid AdminUserId,
    string ActionType,
    string TargetType,
    string? TargetId,
    string PayloadJson);
