using FluentValidation;
using Microsoft.Extensions.Localization;

namespace TTCCashRegister.Data.ItemDetail;

public class ItemDetailValidator : AbstractValidator<ItemDetailModel>
{
    public ItemDetailValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(u => u.CostDetails).NotEmpty()
            .WithMessage(localizer["DetailDescriptionRequired"]);
        RuleFor(c => c.CostDetails).MaximumLength(1000)
            .WithMessage(localizer["DetailDescriptionMaxLength"]);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ItemDetailModel>.CreateWithOptions((ItemDetailModel)model, 
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}