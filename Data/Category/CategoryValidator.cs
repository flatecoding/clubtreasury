using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClubTreasury.Data.Category;

public class CategoryValidator : AbstractValidator<CategoryModel>
{
    public CategoryValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage(localizer["PositionDescriptionRequired"]);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CategoryModel>.CreateWithOptions((CategoryModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}