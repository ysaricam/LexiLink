using LexiLink.Common.Application.Time;
using LexiLink.Modules.Administration.Application.Configuration.Commands;
using LexiLink.Modules.Administration.Domain.AdminUsers;

namespace LexiLink.Modules.Administration.Application.AdminUsers.RegisterAdminUser;

internal class RegisterAdminUserCommandHandler : ICommandHandler<RegisterAdminUserCommand, Guid>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IClock _clock;

    internal RegisterAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IClock clock)
    {
        _adminUserRepository = adminUserRepository;
        _clock = clock;
    }

    public async Task<Guid> Handle(RegisterAdminUserCommand request, CancellationToken cancellationToken)
    {
        // Idempotent on email — the same Email.Of normalization happens both
        // here and in the aggregate's Register path, so a second call with the
        // same address (any case) returns the existing admin's id instead of
        // throwing. Bootstrap seed and re-registration during ops both rely
        // on this.
        var email = Email.Of(request.Email);

        var existing = await _adminUserRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            return existing.Id.Value;
        }

        var adminUser = AdminUser.Register(email, _clock.UtcNow);
        await _adminUserRepository.AddAsync(adminUser, cancellationToken);

        return adminUser.Id.Value;
    }
}
