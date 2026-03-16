using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.Category;

public class CategoryValidator : BaseValidator<CategoryModel>
{
    public CategoryValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage(localizer["PositionDescriptionRequired"]);
    }
}