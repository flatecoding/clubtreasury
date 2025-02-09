using FluentValidation;

namespace TTCCashRegister.Data.SpecialItem;

public class SpecialItemValidator:AbstractValidator<SpecialItemModel>
{
    public SpecialItemValidator()
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage("Position description is required");
        RuleFor(s => s.Betrag).GreaterThanOrEqualTo(decimal.Zero).WithMessage("Sum must be greater than or equal to 0.00");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<SpecialItemModel>.CreateWithOptions((SpecialItemModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
    
}