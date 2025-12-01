using TTCCashRegister.Data.Mapper.DTOs;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.TransactionDetails;

namespace TTCCashRegister.Data.Mapper;

public class BudgetMapper : IBudgetMapper
{
    public BudgetFlatEntryDto MapTransaction(TransactionModel t)
    {
        return new BudgetFlatEntryDto
        {
            CostCenterId = t.Allocation.CostCenter.Id,
            CostCenterName = t.Allocation.CostCenter.CostUnitName,
            CategoryId = t.Allocation.Category.Id,
            CategoryName = t.Allocation.Category.Name,
            ItemDetailId = t.Allocation.ItemDetail?.Id,
            ItemDetailName = t.Allocation.ItemDetail?.CostDetails,
            Amount = t.AccountMovement,
            PersonId = null,
            PersonName = null
        };
    }

    public BudgetFlatEntryDto MapTransactionDetail(TransactionDetailsModel td)
    {
        return new BudgetFlatEntryDto
        {
            CostCenterId = td.Transaction.Allocation.CostCenter.Id,
            CostCenterName = td.Transaction.Allocation.CostCenter.CostUnitName,
            CategoryId = td.Transaction.Allocation.Category.Id,
            CategoryName = td.Transaction.Allocation.Category.Name,
            ItemDetailId = td.Transaction.Allocation.ItemDetail?.Id,
            ItemDetailName = td.Transaction.Allocation.ItemDetail?.CostDetails,
            Amount = td.Sum,
            PersonId = td.Person.Id,
            PersonName = td.Person.Name
        };
    }
    
    public IEnumerable<BudgetFlatEntryDto> BuildFlatEntries(IEnumerable<TransactionModel> transactions)
    {
        var flat = new List<BudgetFlatEntryDto>();

        foreach (var t in transactions)
        {
            if (!t.TransactionDetails.Any())
                flat.Add(MapTransaction(t));

            flat.AddRange(t.TransactionDetails.Select(td => MapTransactionDetail(td)));
        }

        return flat;
    }
    
    public List<BudgetGroupedDto> BuildBudgetHierarchy(IEnumerable<BudgetFlatEntryDto> flatEntries)
{
    return flatEntries
        // 1. Ebene: CostCenter
        .GroupBy(e => new { e.CostCenterId, e.CostCenterName })
        .Select(costCenterGroup => new BudgetGroupedDto
        {
            CostCenterId  = costCenterGroup.Key.CostCenterId,
            CostUnitName  = costCenterGroup.Key.CostCenterName,
            SumCostCenter = costCenterGroup.Sum(e => e.Amount),

            Categories = costCenterGroup
                // 2. Ebene: Kategorie
                .GroupBy(e => new { e.CategoryId, e.CategoryName })
                .Select(categoryGroup => new BudgetCategoryDto
                {
                    CategoryId    = categoryGroup.Key.CategoryId,
                    CategoryName  = categoryGroup.Key.CategoryName,
                    SumCategories = categoryGroup.Sum(e => e.Amount),

                    ItemDetails = categoryGroup
                        // 3. Ebene: ItemDetail
                        .GroupBy(e => new { e.ItemDetailId, e.ItemDetailName })
                        .Select(itemDetailGroup => new BudgetItemDetailDto
                        {
                            ItemDetailId   = itemDetailGroup.Key.ItemDetailId,
                            ItemDetailName = itemDetailGroup.Key.ItemDetailName ?? string.Empty,
                            SumItemDetails = itemDetailGroup.Sum(e => e.Amount),

                            Persons = itemDetailGroup
                                // 4. Ebene: Personen
                                .Where(x => x.PersonId != null)
                                .GroupBy(x => new { x.PersonId, x.PersonName })
                                .Select(p => new BudgetPersonDto
                                {
                                    PersonId   = p.Key.PersonId!.Value,
                                    PersonName = p.Key.PersonName ?? string.Empty,
                                    SumPerson  = p.Sum(x => x.Amount)
                                })
                                .OrderBy(p => p.PersonName)
                                .ToList()
                        })
                        .OrderBy(id => id.ItemDetailName)
                        .ToList()
                })
                .OrderBy(cat => cat.CategoryName)
                .ToList()
        })
        .OrderBy(cc => cc.CostUnitName)
        .ToList();
}

}