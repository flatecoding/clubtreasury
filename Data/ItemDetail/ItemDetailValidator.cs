using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.ItemDetail;

public class ItemDetailValidator : BaseValidator<ItemDetailModel>
{
    public ItemDetailValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(u => u.CostDetails).NotEmpty()
            .WithMessage(localizer["DetailDescriptionRequired"]);
        RuleFor(c => c.CostDetails).MaximumLength(1000)
            .WithMessage(localizer["DetailDescriptionMaxLength"]);
    }
}