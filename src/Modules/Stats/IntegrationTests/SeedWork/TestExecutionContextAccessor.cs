using LexiLink.Common.Application;

namespace LexiLink.Modules.Stats.IntegrationTests.SeedWork;

internal sealed class TestExecutionContextAccessor : IExecutionContextAccessor
{
    public Guid UserId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public bool IsAvailable => true;
    public bool IsAdmin => false;
    public PlayerAuthSessionMode? PlayerAuthSessionMode =>
        LexiLink.Common.Application.PlayerAuthSessionMode.Guest;
    public Guid? AdminUserId => null;
}
