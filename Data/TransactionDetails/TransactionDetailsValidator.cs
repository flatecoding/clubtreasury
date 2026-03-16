using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.TransactionDetails;

public class TransactionDetailsValidator : BaseValidator<TransactionDetailsModel>
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
}