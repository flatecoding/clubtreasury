using FluentValidation;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;

namespace TTCCashRegister.Data.UnitDetail;

public class UnitDetailValidator : AbstractValidator<UnitDetailsModel>
{
    public UnitDetailValidator()
    {
        RuleFor(u => u.CostDetails).NotEmpty().WithMessage("Deteil description is required");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UnitDetailsModel>.CreateWithOptions((UnitDetailsModel)model, 
            x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}