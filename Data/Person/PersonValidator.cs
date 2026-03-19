using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.Person;

public class PersonValidator : BaseValidator<PersonModel>
{
    public PersonValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage(localizer["PersonNameRequired"]);

        RuleFor(p => p.Name)
            .MaximumLength(200).WithMessage(localizer["NameMaxLength200"]);
    }
}