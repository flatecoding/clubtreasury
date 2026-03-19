using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.CashRegister;

public class CashRegisterValidator : BaseValidator<CashRegisterModel>
{
    public CashRegisterValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(cr => cr.Name).NotEmpty().WithMessage(localizer["CashRegisterNameRequired"]);
        RuleFor(cr => cr.Name).MaximumLength(150).WithMessage(localizer["NameMaxLength150"]);
        RuleFor(cr => cr.FiscalYearStartMonth).InclusiveBetween(1, 12);
    }
}