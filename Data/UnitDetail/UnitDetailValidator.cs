using FluentValidation;

namespace TTCCashRegister.Data.UnitDetail;

public class UnitDetailValidator : AbstractValidator<UnitDetailsModel>
{
    public UnitDetailValidator()
    {
        RuleFor(u => u.CostDetails).NotEmpty().WithMessage("Detail description is required");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UnitDetailsModel>.CreateWithOptions((UnitDetailsModel)model, 
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}