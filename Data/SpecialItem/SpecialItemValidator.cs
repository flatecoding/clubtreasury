using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.SpecialItem;

public class SpecialItemValidator : BaseValidator<SpecialItemModel>
{
    public SpecialItemValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage(localizer["PositionDescriptionRequired"]);
        RuleFor(sp => sp.Name).MaximumLength(50).WithMessage(localizer["SpecialPositionNameTooLong"]);
    }
}