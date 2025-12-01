using TTCCashRegister.Data.Mapper.DTOs;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.TransactionDetails;

namespace TTCCashRegister.Data.Mapper;

public interface IBudgetMapper
{
    BudgetFlatEntryDto MapTransaction(TransactionModel t);
    BudgetFlatEntryDto MapTransactionDetail(TransactionDetailsModel td);
    IEnumerable<BudgetFlatEntryDto> BuildFlatEntries(IEnumerable<TransactionModel> transactions);
    List<BudgetGroupedDto> BuildBudgetHierarchy(IEnumerable<BudgetFlatEntryDto> flatEntries);
}