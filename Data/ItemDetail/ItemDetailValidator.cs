using FluentValidation;

namespace TTCCashRegister.Data.ItemDetail;

public class ItemDetailValidator : AbstractValidator<ItemDetailModel>
{
    public ItemDetailValidator()
    {
        RuleFor(u => u.CostDetails).NotEmpty()
            .WithMessage("Detail description is required");
        RuleFor(c => c.CostDetails).MaximumLength(1000)
            .WithMessage("Detail description maximum length exceeded");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ItemDetailModel>.CreateWithOptions((ItemDetailModel)model, 
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}