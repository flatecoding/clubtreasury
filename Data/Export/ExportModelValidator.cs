using FluentValidation;
using Microsoft.Extensions.Localization;

namespace TTCCashRegister.Data.Export;

public class ExportModelValidator : AbstractValidator<ExportModel>
{
    public ExportModelValidator(IStringLocalizer<Translation> localizer)
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage(localizer["FileNameRequired"]);

        RuleFor(x => x.DateRange)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(localizer["DateRangeRequired"])
            .Must(dateRange => dateRange!.Start.HasValue && dateRange.End.HasValue)
            .WithMessage(localizer["DateRangeRequired"])
            .Must(dateRange => dateRange!.Start!.Value.Date <= dateRange.End!.Value.Date)
            .WithMessage(localizer["DateRangeError"]);
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ExportModel>.CreateWithOptions((ExportModel)model, 
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}