using MudBlazor;

namespace ClubTreasury.Data.Transaction.Dialogs;

public static class DateRangeSanitizer
{
    public static DateRange Sanitize(DateRange dateRange)
    {
        var start = IsValidDate(dateRange.Start) ? dateRange.Start : null;
        var end = IsValidDate(dateRange.End) ? dateRange.End : null;

        return new DateRange(start, end);
    }

    private static bool IsValidDate(DateTime? date)
    {
        if (date is null)
            return true;

        return date.Value.Year is >= 1900 and <= 2100;
    }
}