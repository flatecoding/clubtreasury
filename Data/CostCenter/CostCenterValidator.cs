using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.CostCenter;

public class CostCenterValidator : BaseValidator<CostCenterModel>
{
    public CostCenterValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(n => n.CostUnitName).NotEmpty().WithMessage(localizer["CostUnitNameRequired"]);
        RuleFor(n => n.CostUnitName).Length(1, 100).WithMessage(localizer["CostUnitNameLength"]);
    }
}