using FluentValidation;
using Microsoft.Extensions.Localization;

namespace TTCCashRegister.Data.Person
{
    public class PersonValidator : AbstractValidator<PersonModel>
    {
        public PersonValidator(IStringLocalizer<Translation> localizer)
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage(localizer["PersonNameRequired"]);

            RuleFor(p => p.Name)
                .MaximumLength(200).WithMessage(localizer["NameMaxLength200"]);
        }
        
        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(
                ValidationContext<PersonModel>.CreateWithOptions(
                    (PersonModel)model, 
                    x => x.IncludeProperties(propertyName)
                )
            );
            return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
        };
    }
}