using FluentValidation;
using Microsoft.Extensions.Localization;

namespace TTCCashRegister.Data.CashRegister;

public class CashRegisterValidator :AbstractValidator<CashRegisterModel>
{
    public CashRegisterValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(cr => cr.Name).NotEmpty().WithMessage(localizer["CashRegisterNameRequired"]);
        RuleFor(cr => cr.Name).MaximumLength(150).WithMessage(localizer["NameMaxLength150"]);
        RuleFor(cr => cr.FiscalYearStartMonth).InclusiveBetween(1, 12);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CashRegisterModel>.CreateWithOptions((CashRegisterModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };

}