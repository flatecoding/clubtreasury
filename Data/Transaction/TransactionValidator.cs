using FluentValidation;

namespace TTCCashRegister.Data.Transaction;

public class TransactionValidator : AbstractValidator<TransactionModel>
{
    public TransactionValidator()
    {
        RuleFor(t =>t.CashRegister).NotNull().WithMessage("CashRegister is required.");
        RuleFor(t => t.Documentnumber).NotEmpty().WithMessage("Document number is required");
        RuleFor(t => t.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(t => t.Sum).GreaterThanOrEqualTo(0.01m).WithMessage("Sum must be greater than or equal to 0.01");
        RuleFor(t => t.AccountMovement).NotEmpty().WithMessage("Account Movement is required");
        RuleFor(t => t.CostUnit).NotNull().WithMessage("Cost Unit is required");
        RuleFor(t => t.BasicUnit).NotNull().WithMessage("Basic Unit is required");
        RuleFor(t => t.SpecialItemId).NotEmpty().WithMessage("Special Item is required");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<TransactionModel>.CreateWithOptions((TransactionModel)model, 
            x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
    
}