namespace LexiLink.Modules.Payments.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class PaymentsIntegrationProjectSmokeTests
{
    [Test]
    public void Payments_Integration_Project_Should_Load()
    {
        typeof(Infrastructure.PaymentsContext).Should().NotBeNull();
    }
}
