using LexiLink.Common.Application;

namespace LexiLink.Modules.Hint.IntegrationTests.SeedWork;

internal class TestExecutionContextAccessor : IExecutionContextAccessor
{
    public Guid UserId => Guid.Empty;
    public Guid CorrelationId => Guid.Empty;
    public bool IsAvailable => false;
    public bool IsAdmin => false;
    public Guid? AdminUserId => null;
}
