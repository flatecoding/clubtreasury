using MudBlazor;

namespace TTCCashRegister.Data.Transaction.Dialogs;

public class TransactionFilterValues
{
    public DateRange DateRange { get; init; } = new(null, null);
    public string SearchText { get; init; } = string.Empty;
    public int? PersonId { get; init; }
    
}