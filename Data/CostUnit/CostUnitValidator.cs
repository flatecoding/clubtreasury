using System.Data;
using FluentValidation;

namespace TTCCashRegister.Data.CostUnit;

public class CostUnitValidator : AbstractValidator<CostUnitModel>
{
    public CostUnitValidator()
    {
        RuleFor(n => n.CostUnitName).NotEmpty().WithMessage("CostUnit Name is required.");
        RuleFor(n => n.CostUnitName).Length(1, 100).WithMessage("CostUnit Name must be between 1 and 100 characters.");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CostUnitModel>.CreateWithOptions((CostUnitModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}