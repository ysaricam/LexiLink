using FluentValidation;
using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;

internal class UpdatePlayerProfileCommandValidator : AbstractValidator<UpdatePlayerProfileCommand>
{
    public UpdatePlayerProfileCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Locale).NotEmpty();
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MinimumLength(DisplayNameMustMeetMinimumLengthRule.MinLength)
            .MaximumLength(DisplayNameMustNotExceedMaxLengthRule.MaxLength)
            .Must(displayName => displayName?.Contains('#') != true)
            .WithMessage("Player display name cannot contain '#'.")
            .When(x => x.DisplayName is not null || x.Discriminator is not null);
        RuleFor(x => x.Discriminator)
            .NotNull()
            .InclusiveBetween(Discriminator.MinValue, Discriminator.MaxValue)
            .When(x => x.DisplayName is not null || x.Discriminator is not null);
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(AvatarUrlMustBeValidIfProvidedRule.MaxLength)
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
    }
}
