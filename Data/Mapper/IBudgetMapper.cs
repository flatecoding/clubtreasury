using ClubTreasury.Data.Mapper.DTOs;
using ClubTreasury.Data.Transaction;
using ClubTreasury.Data.TransactionDetails;

namespace ClubTreasury.Data.Mapper;

public interface IBudgetMapper
{
    BudgetFlatEntryDto MapTransaction(TransactionModel t);
    BudgetFlatEntryDto MapTransactionDetail(TransactionDetailsModel td);
    IEnumerable<BudgetFlatEntryDto> BuildFlatEntries(IEnumerable<TransactionModel> transactions);
    List<BudgetGroupedDto> BuildBudgetHierarchy(IEnumerable<BudgetFlatEntryDto> flatEntries);
}