using FluentValidation;
namespace TTCCashRegister.Data.BasicUnit;

public class BasicUnitValidator : AbstractValidator<BasicUnitModel>
{
    public BasicUnitValidator()
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage("Position description is required");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<BasicUnitModel>.CreateWithOptions((BasicUnitModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}