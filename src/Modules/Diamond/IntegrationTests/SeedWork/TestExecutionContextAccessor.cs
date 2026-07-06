using LexiLink.Common.Application;

namespace LexiLink.Modules.Diamond.IntegrationTests.SeedWork;

internal class TestExecutionContextAccessor : IExecutionContextAccessor
{
    public Guid UserId => Guid.Empty;
    public Guid CorrelationId => Guid.Empty;
    public bool IsAvailable => false;
    public bool IsAdmin => false;
    public PlayerAuthSessionMode? PlayerAuthSessionMode => null;
    public Guid? AdminUserId => null;
}
