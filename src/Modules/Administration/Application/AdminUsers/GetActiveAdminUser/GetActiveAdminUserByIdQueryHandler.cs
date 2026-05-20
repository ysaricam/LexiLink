using LexiLink.Modules.Administration.Application.Configuration.Queries;
using LexiLink.Modules.Administration.Domain.AdminUsers;

namespace LexiLink.Modules.Administration.Application.AdminUsers.GetActiveAdminUser;

internal sealed class GetActiveAdminUserByIdQueryHandler : IQueryHandler<GetActiveAdminUserByIdQuery, AdminUserDto?>
{
    private readonly IAdminUserRepository _adminUserRepository;

    internal GetActiveAdminUserByIdQueryHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task<AdminUserDto?> Handle(
        GetActiveAdminUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var adminUser = await _adminUserRepository.GetByIdAsync(
            new AdminUserId(request.AdminUserId),
            cancellationToken);

        if (adminUser is null || !adminUser.IsActive)
        {
            return null;
        }

        return new AdminUserDto(
            adminUser.Id.Value,
            adminUser.Email.Value,
            adminUser.Role.Value);
    }
}
