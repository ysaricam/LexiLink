using FluentValidation;

namespace LexiLink.Modules.Administration.Application.AdminUsers.RegisterAdminUser;

internal class RegisterAdminUserCommandValidator : AbstractValidator<RegisterAdminUserCommand>
{
    public RegisterAdminUserCommandValidator()
    {
        // Domain-level rules (Email.Of) re-check empty/format with full
        // business-rule machinery. Validator is the API-shape guard so admin
        // surfaces report 400 ProblemDetails before reaching the aggregate.
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
