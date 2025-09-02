using FluentValidation;

namespace TTCCashRegister.Data.Person
{
    public class PersonValidator : AbstractValidator<PersonModel>
    {
        public PersonValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Name for person is required.");
            
            RuleFor(p => p.Name)
                .MaximumLength(200).WithMessage("Name max length is 200 characters.");
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