using FluentValidation;

namespace TTCCashRegister.Data.CashRegister;

public class CashRegisterValidator :AbstractValidator<CashRegisterModel>
{
    public CashRegisterValidator()
    {
        RuleFor(cr => cr.Name).NotEmpty().WithMessage("Position description is required");
        RuleFor(cr => cr.CurrentBalance).GreaterThanOrEqualTo(decimal.Zero).WithMessage("Sum must be greater than or equal to 0.00");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CashRegisterModel>.CreateWithOptions((CashRegisterModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };

}