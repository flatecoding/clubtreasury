namespace TTCCashRegister.Data.Export;

public class BudgetLine
{
    public BudgetLineType LineType { get; set; }

    public string? CostCenter { get; set; }
    public string? Category { get; set; }
    public string? DetailOrPerson { get; set; }
    public decimal Amount { get; set; }

    public BudgetLine(BudgetLineType type)
    {
        LineType = type;
    }
}