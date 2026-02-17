using FluentValidation;

namespace TTCCashRegister.Data.CashRegister;

public class CashRegisterValidator :AbstractValidator<CashRegisterModel>
{
    public CashRegisterValidator()
    {
        RuleFor(cr => cr.Name).NotEmpty().WithMessage("Name for cash register is required.");
        RuleFor(cr => cr.Name).MaximumLength(150).WithMessage("Name max length is 150 characters.");
        RuleFor(cr => cr.FiscalYearStartMonth).InclusiveBetween(1, 12);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CashRegisterModel>.CreateWithOptions((CashRegisterModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };

}