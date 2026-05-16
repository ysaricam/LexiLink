using FluentValidation;

namespace LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;

internal class ClaimQuestCommandValidator : AbstractValidator<ClaimQuestCommand>
{
    public ClaimQuestCommandValidator()
    {
        RuleFor(x => x.PlayerQuestId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
