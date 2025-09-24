using FluentValidation;

namespace TTCCashRegister.Data.Category;

public class CategoryValidator : AbstractValidator<CategoryModel>
{
    public CategoryValidator()
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage("Position description is required");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CategoryModel>.CreateWithOptions((CategoryModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}