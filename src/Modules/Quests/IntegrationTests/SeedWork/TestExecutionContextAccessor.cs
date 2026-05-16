using LexiLink.Common.Application;

namespace LexiLink.Modules.Quests.IntegrationTests.SeedWork;

internal class TestExecutionContextAccessor : IExecutionContextAccessor
{
    public Guid UserId => Guid.Empty;
    public Guid CorrelationId => Guid.Empty;
    public bool IsAvailable => false;
}
