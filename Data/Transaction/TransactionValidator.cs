using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.Transaction;

public class TransactionValidator : BaseValidator<TransactionModel>
{
    public TransactionValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(t => t.Documentnumber).NotEmpty().WithMessage(localizer["DocumentNumberRequired"]);
        RuleFor(t => t.Description).NotEmpty().WithMessage(localizer["DescriptionRequired"]);
        RuleFor(t => t.Sum).GreaterThanOrEqualTo(0.01m).WithMessage(localizer["SumMinValue"]);
        RuleFor(t => t.AccountMovement).NotEmpty().WithMessage(localizer["AccountMovementRequired"]);
        RuleFor(t => t.AccountMovement)
            .Must((t, accountMovement) => Math.Abs(accountMovement) == t.Sum)
            .WithMessage(localizer["SumAccountMismatch"])
            .WithSeverity(Severity.Warning);
    }
}