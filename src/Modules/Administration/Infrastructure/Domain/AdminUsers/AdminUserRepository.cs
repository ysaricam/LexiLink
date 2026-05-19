using LexiLink.Modules.Administration.Domain.AdminUsers;
using Microsoft.EntityFrameworkCore;

namespace LexiLink.Modules.Administration.Infrastructure.Domain.AdminUsers;

internal class AdminUserRepository : IAdminUserRepository
{
    private readonly AdministrationContext _administrationContext;

    internal AdminUserRepository(AdministrationContext administrationContext)
    {
        _administrationContext = administrationContext;
    }

    public async Task<AdminUser?> GetByIdAsync(AdminUserId id, CancellationToken cancellationToken = default)
    {
        return await _administrationContext.AdminUsers
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<AdminUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        // OwnsOne value-object comparisons don't translate to SQL. Compare the
        // backing column instead. Email.Of() normalizes to lowercase, so a
        // direct string compare is correct.
        return await _administrationContext.AdminUsers
            .FirstOrDefaultAsync(
                x => EF.Property<string>(EF.Property<Email>(x, "_email"), "Value") == email.Value,
                cancellationToken);
    }

    public async Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken = default)
    {
        await _administrationContext.AdminUsers.AddAsync(adminUser, cancellationToken);
    }
}
