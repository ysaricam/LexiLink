using LexiLink.Modules.Administration.Application.AdminUsers.RegisterAdminUser;
using LexiLink.Modules.Administration.Application.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace LexiLink.API.Configuration.Bootstrap;

/// <summary>
/// Idempotently ensures the admin users listed in
/// `Administration:Bootstrap:AdminEmails` exist on every API start. The
/// list is env/config-driven so production and CI can each supply their
/// own without hardcoding identifiers anywhere. The same email registered
/// twice short-circuits to the existing aggregate id (see
/// `RegisterAdminUserCommandHandler` idempotency contract).
/// </summary>
internal sealed class AdministrationBootstrapHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<AdministrationBootstrapOptions> _options;
    private readonly ILogger _logger;

    public AdministrationBootstrapHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<AdministrationBootstrapOptions> options,
        ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var emails = _options.Value.AdminEmails ?? [];
        if (emails.Length == 0)
        {
            _logger.Information(
                "Administration bootstrap: no admin emails configured ({Section}:AdminEmails). Skipping.",
                AdministrationBootstrapOptions.SectionName);
            return;
        }

        foreach (var email in emails)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                continue;
            }

            // One scope per command — each command owns a fresh DbContext +
            // UnitOfWork. Sharing a scope across commands would let EF carry
            // OwnsOne shadow-FK state between aggregates and trigger
            // identifying-foreign-key conflicts on the second insert.
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var administration = scope.ServiceProvider.GetRequiredService<IAdministrationModule>();

                var adminId = await administration.ExecuteCommandAsync(
                    new RegisterAdminUserCommand(email),
                    cancellationToken);

                _logger.Information(
                    "Administration bootstrap: ensured admin user {Email} -> {AdminUserId}",
                    email,
                    adminId);
            }
            catch (Exception ex)
            {
                // Bootstrap is best-effort — log and continue so a single bad
                // entry doesn't keep the API from starting. Operators can
                // re-run by restarting after fixing config.
                _logger.Error(
                    ex,
                    "Administration bootstrap: failed to ensure admin user {Email}",
                    email);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
