using System.Globalization;

namespace ClubTreasury.Data.Export.Budget;

internal static class BudgetExportFormats
{
    public static string CurrencyFormat
    {
        get
        {
            var nfi = CultureInfo.CurrentCulture.NumberFormat;
            var digits = new string('0', nfi.CurrencyDecimalDigits);
            var symbol = nfi.CurrencySymbol;
            return nfi.CurrencyPositivePattern switch
            {
                0 => $"\"{symbol}\"#,##0.{digits}",
                1 => $"#,##0.{digits}\"{symbol}\"",
                2 => $"\"{symbol}\" #,##0.{digits}",
                _ => $"#,##0.{digits} \"{symbol}\""
            };
        }
    }
}