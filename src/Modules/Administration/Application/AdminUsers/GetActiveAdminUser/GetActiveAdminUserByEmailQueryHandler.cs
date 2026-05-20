using LexiLink.Modules.Administration.Application.Configuration.Queries;
using LexiLink.Modules.Administration.Domain.AdminUsers;

namespace LexiLink.Modules.Administration.Application.AdminUsers.GetActiveAdminUser;

internal sealed class GetActiveAdminUserByEmailQueryHandler : IQueryHandler<GetActiveAdminUserByEmailQuery, AdminUserDto?>
{
    private readonly IAdminUserRepository _adminUserRepository;

    internal GetActiveAdminUserByEmailQueryHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task<AdminUserDto?> Handle(
        GetActiveAdminUserByEmailQuery request,
        CancellationToken cancellationToken)
    {
        // Email.Of normalizes + validates — bad inputs surface as
        // BusinessRuleValidationException, which the exception middleware
        // converts to 400 ProblemDetails. Lookups for unknown but
        // well-formed emails return null.
        var email = Email.Of(request.Email);

        var adminUser = await _adminUserRepository.GetByEmailAsync(email, cancellationToken);
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
