using FluentValidation;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;

internal sealed class UpdateQuestDefinitionCommandValidator : AbstractValidator<UpdateQuestDefinitionCommand>
{
    public UpdateQuestDefinitionCommandValidator()
    {
        RuleFor(x => x.QuestDefinitionId).NotEmpty();
        RuleFor(x => x.Description).NotNull().MaximumLength(QuestDescriptionMustNotExceedMaxLengthRule.MaxLength);
        RuleFor(x => x.ProgressBaseline).IsInEnum();
        RuleFor(x => x.Threshold).GreaterThan(0);
        RuleFor(x => x.EnergyReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HintReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UndoReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ResetReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x =>
                x.EnergyReward > 0 ||
                x.HintReward > 0 ||
                x.UndoReward > 0 ||
                x.ResetReward > 0)
            .WithMessage("At least one reward amount must be positive.");
    }
}
