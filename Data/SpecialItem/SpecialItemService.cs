using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.SpecialItem
{
    public class SpecialItemService(CashDataContext context)
    {
        public async Task<List<SpecialItemModel>> GetAllSpecialItems()
        {
            return await context.SpecialItems
                .Include(s => s.Transactions)  
                .ToListAsync();
        }

        public async Task<SpecialItemModel?> GetSpecialPositionById(int id)
        {
            return await context.SpecialItems
                .Include(s => s.Transactions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddSpecialPosition(SpecialItemModel specialPosition)
        {
            try
            {
                await context.SpecialItems.AddAsync(specialPosition);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        public async Task UpdateSpecialPosition(SpecialItemModel specialPosition)
        {
            try
            {
                context.SpecialItems.Update(specialPosition);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        public async Task<bool> DeleteSpecialPosition(int id)
        {
            try
            {
                var specialPosition = await context.SpecialItems.FindAsync(id);
                if (specialPosition is null)
                {
                    return false;
                }

                context.SpecialItems.Remove(specialPosition);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }
    }
}
