using FluentValidation;
using Microsoft.Extensions.Localization;

namespace TTCCashRegister.Data.SpecialItem;

public class SpecialItemValidator:AbstractValidator<SpecialItemModel>
{
    public SpecialItemValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(s => s.Name).NotEmpty().WithMessage(localizer["PositionDescriptionRequired"]);
        RuleFor(sp => sp.Name).MaximumLength(50).WithMessage(localizer["SpecialPositionNameTooLong"]);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<SpecialItemModel>.CreateWithOptions((SpecialItemModel)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
    
}