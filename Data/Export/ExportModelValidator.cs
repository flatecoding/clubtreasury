using FluentValidation;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Validation;

namespace ClubTreasury.Data.Export;

public class ExportModelValidator : BaseValidator<ExportModel>
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
}