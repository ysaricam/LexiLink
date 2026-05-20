using LexiLink.Modules.Administration.Application.AdminUsers.GetActiveAdminUser;
using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.API.CrossModule;

internal sealed class AdminLookup : IAdminLookup
{
    private readonly IAdministrationModule _administration;

    public AdminLookup(IAdministrationModule administration)
    {
        _administration = administration;
    }

    public async Task<AdminLookupResult?> FindActiveByIdAsync(
        Guid adminUserId,
        CancellationToken cancellationToken = default)
    {
        var dto = await _administration.ExecuteQueryAsync(
            new GetActiveAdminUserByIdQuery(adminUserId),
            cancellationToken);

        return dto is null
            ? null
            : new AdminLookupResult(dto.Id, dto.Email, dto.Role);
    }

    public async Task<AdminLookupResult?> FindActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var dto = await _administration.ExecuteQueryAsync(
            new GetActiveAdminUserByEmailQuery(email),
            cancellationToken);

        return dto is null
            ? null
            : new AdminLookupResult(dto.Id, dto.Email, dto.Role);
    }
}
