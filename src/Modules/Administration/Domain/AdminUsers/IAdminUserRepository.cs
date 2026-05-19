using LexiLink.Common.Domain;

namespace LexiLink.Modules.Administration.Domain.AdminUsers;

public interface IAdminUserRepository : IRepository<AdminUser>
{
    Task<AdminUser?> GetByIdAsync(AdminUserId id, CancellationToken cancellationToken = default);

    Task<AdminUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken = default);
}
