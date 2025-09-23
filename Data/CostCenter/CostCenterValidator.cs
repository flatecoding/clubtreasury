using FluentValidation;

namespace TTCCashRegister.Data.CostCenter;

public class CostCenterValidator : AbstractValidator<CostCenterModel>
{
    public CostCenterValidator()
    {
        RuleFor(n => n.CostUnitName).NotEmpty().WithMessage("CostUnit Name is required.");
        RuleFor(n => n.CostUnitName).Length(1, 100).WithMessage("CostUnit Name must be between 1 and 100 characters.");
    }
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CostCenterModel>.CreateWithOptions((CostCenterModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}