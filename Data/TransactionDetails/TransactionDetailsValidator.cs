using FluentValidation;

namespace TTCCashRegister.Data.TransactionDetails
{
    public class TransactionDetailsValidator : AbstractValidator<TransactionDetailsModel>
    {
        public TransactionDetailsValidator()
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
                ValidationContext<TransactionDetailsModel>.CreateWithOptions(
                    (TransactionDetailsModel)model,
                    x => x.IncludeProperties(propertyName)
                )
            );
            return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
        };
    }
}