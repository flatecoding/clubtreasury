using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClubTreasury.Data.Allocation;

public class AllocationValidator: AbstractValidator<AllocationModel>
{
    public AllocationValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(a => a.CategoryId).GreaterThan(0).WithMessage(localizer["CategoryRequired"]);
        RuleFor(a => a.CostCenterId).GreaterThan(0).WithMessage(localizer["CostCenterRequired"]);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<AllocationModel>.CreateWithOptions((AllocationModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}