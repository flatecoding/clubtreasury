using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClubTreasury.Data.CostCenter;

public class CostCenterValidator : AbstractValidator<CostCenterModel>
{
    public CostCenterValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(n => n.CostUnitName).NotEmpty().WithMessage(localizer["CostUnitNameRequired"]);
        RuleFor(n => n.CostUnitName).Length(1, 100).WithMessage(localizer["CostUnitNameLength"]);
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CostCenterModel>.CreateWithOptions((CostCenterModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}