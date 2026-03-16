using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.Allocation;

public class AllocationValidator : BaseValidator<AllocationModel>
{
    public AllocationValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(a => a.CategoryId).GreaterThan(0).WithMessage(localizer["CategoryRequired"]);
        RuleFor(a => a.CostCenterId).GreaterThan(0).WithMessage(localizer["CostCenterRequired"]);
    }
}