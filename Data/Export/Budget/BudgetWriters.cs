namespace ClubTreasury.Data.Export.Budget;

public class BudgetWriters(
    ICsvBudgetWriter csv,
    IExcelBudgetWriter excel,
    IBudgetPlanExcelWriter budgetPlan) : IBudgetWriters
{
    public ICsvBudgetWriter Csv { get; } = csv;
    public IExcelBudgetWriter Excel { get; } = excel;
    public IBudgetPlanExcelWriter BudgetPlan { get; } = budgetPlan;
}