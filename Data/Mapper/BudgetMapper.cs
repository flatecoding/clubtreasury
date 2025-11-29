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
}