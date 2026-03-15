using Microsoft.EntityFrameworkCore;

namespace ClubTreasury.Data.Transaction;

public static class TransactionQueryExtensions
{
    extension(IQueryable<TransactionModel> query)
    {
        public IQueryable<TransactionModel> WithAllocationDetails()
        {
            return query
                .Include(t => t.Allocation).ThenInclude(a => a.CostCenter)
                .Include(t => t.Allocation).ThenInclude(a => a.Category)
                .Include(t => t.Allocation).ThenInclude(a => a.ItemDetail);
        }

        public IQueryable<TransactionModel> WithTransactionDetailsAndPersons()
        {
            return query
                .Include(t => t.TransactionDetails).ThenInclude(td => td.Person);
        }
    }
}