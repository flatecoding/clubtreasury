using MudBlazor;

namespace TTCCashRegister.Data.Export;

public class ExportModel
{
    public DateRange? DateRange { get; set; }
    public string FileName { get; set; } = string.Empty;
}