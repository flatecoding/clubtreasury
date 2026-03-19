using Microsoft.EntityFrameworkCore;

namespace ClubTreasury.Data.Allocation;

public static class AllocationQueryExtensions
{
    extension(IQueryable<AllocationModel> query)
    {
        public IQueryable<AllocationModel> WithAllComponents()
        {
            return query
                .Include(a => a.CostCenter)
                .Include(a => a.Category)
                .Include(a => a.ItemDetail);
        }
    }
}