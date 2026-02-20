using FluentValidation;
using Microsoft.Extensions.Localization;

namespace TTCCashRegister.Data.Transaction;

public class TransactionValidator : AbstractValidator<TransactionModel>
{
    public TransactionValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(t => t.Documentnumber).NotEmpty().WithMessage(localizer["DocumentNumberRequired"]);
        RuleFor(t => t.Description).NotEmpty().WithMessage(localizer["DescriptionRequired"]);
        RuleFor(t => t.Sum).GreaterThanOrEqualTo(0.01m).WithMessage(localizer["SumMinValue"]);
        RuleFor(t => t.AccountMovement).NotEmpty().WithMessage(localizer["AccountMovementRequired"]);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<TransactionModel>.CreateWithOptions((TransactionModel)model, 
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
    
}