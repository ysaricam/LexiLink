using FluentValidation;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;

internal class UpdatePlayerProfileCommandValidator : AbstractValidator<UpdatePlayerProfileCommand>
{
    public UpdatePlayerProfileCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Locale).NotEmpty();
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(AvatarUrlMustBeValidIfProvidedRule.MaxLength)
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
    }
}
