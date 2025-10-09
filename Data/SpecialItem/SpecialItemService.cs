using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.SpecialItem
{
    public class SpecialItemService(CashDataContext context, ILogger<SpecialItemService> logger)
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
                logger.LogError("Special position added: {@SpecialPosition}", specialPosition);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Special position could not be added: {@SpecialPosition}", specialPosition);
            }
        }

        public async Task UpdateSpecialPosition(SpecialItemModel specialPosition)
        {
            try
            {
                context.SpecialItems.Update(specialPosition);
                await context.SaveChangesAsync();
                logger.LogInformation("Special position updated: {@SpecialPosition}", specialPosition);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Special position could not be updated: {@SpecialPosition}", specialPosition);
            }
        }

        public async Task<bool> DeleteSpecialPosition(int id)
        {
            try
            {
                var specialPosition = await context.SpecialItems.FindAsync(id);
                if (specialPosition is null)
                {
                    logger.LogError("Special position with id '{Id}' to delete not found", id);
                    return false;
                }

                context.SpecialItems.Remove(specialPosition);
                await context.SaveChangesAsync();
                logger.LogInformation("Special position deleted: {@SpecialPosition}", specialPosition);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Special position with id '{Id}' could not be deleted", id);
                return false;
            }
        }
    }
}
