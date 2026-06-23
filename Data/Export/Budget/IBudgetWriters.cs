namespace ClubTreasury.Data.Export.Budget;

public interface IBudgetWriters
{
    ICsvBudgetWriter Csv { get; }
    IExcelBudgetWriter Excel { get; }
    IBudgetPlanExcelWriter BudgetPlan { get; }
}