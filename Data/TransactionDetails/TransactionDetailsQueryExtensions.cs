using Microsoft.EntityFrameworkCore;

namespace ClubTreasury.Data.TransactionDetails;

public static class TransactionDetailsQueryExtensions
{
    extension(IQueryable<TransactionDetailsModel> query)
    {
        public IQueryable<TransactionDetailsModel> WithTransactionAndPerson()
        {
            return query
                .Include(st => st.Transaction)
                .Include(st => st.Person);
        }
    }
}