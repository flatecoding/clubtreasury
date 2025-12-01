using TTCCashRegister.Data.Mapper.DTOs;

namespace TTCCashRegister.Data.Export;

public class BudgetLineBuilder
{
    public static IEnumerable<BudgetLine> EnumerateBudgetLines(IEnumerable<BudgetGroupedDto> grouped)
    {
        foreach (var cc in grouped)
        {
            // ---------------- CostCenter ----------------
            yield return new BudgetLine(BudgetLineType.CostCenter)
            {
                CostCenter = cc.CostUnitName,
                Amount = cc.SumCostCenter
            };

            foreach (var cat in cc.Categories)
            {
                // ---------------- Category ----------------
                yield return new BudgetLine(BudgetLineType.Category)
                {
                    CostCenter = cc.CostUnitName,
                    Category = cat.CategoryName,
                    Amount = cat.SumCategories
                };

                foreach (var item in cat.ItemDetails)
                {
                    // ---------------- ItemDetail ----------------
                    if (!string.IsNullOrWhiteSpace(item.ItemDetailName)
                        || item.Persons.Count == 0
                        || item.Persons.Sum(p => p.SumPerson) != item.SumItemDetails)
                    {
                        yield return new BudgetLine(BudgetLineType.ItemDetail)
                        {
                            CostCenter = cc.CostUnitName,
                            Category = cat.CategoryName,
                            DetailOrPerson = item.ItemDetailName,
                            Amount = item.SumItemDetails
                        };
                    }

                    // ---------------- Person ----------------
                    foreach (var person in item.Persons)
                    {
                        yield return new BudgetLine(BudgetLineType.Person)
                        {
                            CostCenter = cc.CostUnitName,
                            Category = cat.CategoryName,
                            DetailOrPerson = $"  {person.PersonName}",
                            Amount = person.SumPerson
                        };
                    }
                }
            }
        }
    }
}