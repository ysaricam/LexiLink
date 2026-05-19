using LexiLink.Modules.Administration.Application.AdminUsers.RegisterAdminUser;

namespace LexiLink.Modules.Administration.Tests.AdminUsers;

[TestFixture]
public class RegisterAdminUserCommandValidatorTests
{
    private RegisterAdminUserCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterAdminUserCommandValidator();
    }

    [Test]
    public void Validator_Should_Accept_WellFormedEmail()
    {
        var result = _validator.Validate(new RegisterAdminUserCommand("ops@lexilink.test"));

        result.IsValid.Should().BeTrue();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Validator_Should_Reject_EmptyEmail(string email)
    {
        var result = _validator.Validate(new RegisterAdminUserCommand(email));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterAdminUserCommand.Email));
    }

    [TestCase("not-an-email")]
    [TestCase("@nohost")]
    public void Validator_Should_Reject_InvalidFormat(string email)
    {
        var result = _validator.Validate(new RegisterAdminUserCommand(email));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterAdminUserCommand.Email));
    }
}
