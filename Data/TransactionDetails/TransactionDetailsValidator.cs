using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClubTreasury.Data.TransactionDetails
{
    public class TransactionDetailsValidator : AbstractValidator<TransactionDetailsModel>
    {
        public TransactionDetailsValidator(IStringLocalizer<Translation> localizer)
        {
            RuleFor(st => st.TransactionId)
                .NotEmpty()
                .WithMessage(localizer["TransactionReferenceRequired"]);

            RuleFor(st => st.Description)
                .MaximumLength(300)
                .WithMessage(localizer["DescriptionMaxLength300"]);

            RuleFor(st => st.Sum)
                .NotEmpty().WithMessage(localizer["SumRequired"]);
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