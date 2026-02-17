using FluentValidation;

namespace TTCCashRegister.Data.Allocation;

public class AllocationValidator: AbstractValidator<AllocationModel>
{
    public AllocationValidator()
    {
        RuleFor(a => a.CategoryId).GreaterThan(0).WithMessage("Category is required.");
        RuleFor(a => a.CostCenterId).GreaterThan(0).WithMessage("Cost center is required.");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<AllocationModel>.CreateWithOptions((AllocationModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}