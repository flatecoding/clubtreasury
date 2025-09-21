using FluentValidation;

namespace TTCCashRegister.Data.SubTransaction
{
    public class SubTransactionValidator : AbstractValidator<SubTransactionModel>
    {
        public SubTransactionValidator()
        {
            RuleFor(st => st.TransactionId)
                .NotEmpty()
                .WithMessage("Transaction reference is required.");
            
            RuleFor(st => st.Description)
                .MaximumLength(300)
                .WithMessage("Description max length is 300 characters.");
            
            RuleFor(st => st.Sum)
                .NotEmpty().WithMessage("Sum is required.");
        }
        
        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(
                ValidationContext<SubTransactionModel>.CreateWithOptions(
                    (SubTransactionModel)model,
                    x => x.IncludeProperties(propertyName)
                )
            );
            return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
        };
    }
}