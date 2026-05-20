namespace LexiLink.Modules.Administration.Application.AdminUsers.GetActiveAdminUser;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string Role);
