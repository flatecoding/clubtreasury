using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.ItemDetail
{
    public class ItemDetailService(CashDataContext context, ILogger<ItemDetailService> logger)
    {
        private readonly CashDataContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<ItemDetailModel>> GetAllItemDetailsAsync()
        {
            return await _context.ItemDetails
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.Category)
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.CostCenter)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<ItemDetailModel?> GetItemDetailByIdAsync(int id)
        {
            return await _context.ItemDetails
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.Category)
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.CostCenter)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId)
        {
            return await _context.ItemDetails
                .Where(u => u.Allocations.Any(a => a.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task<ItemDetailModel?> AddItemDetailAsync(ItemDetailModel itemDetail)
        {
            try
            {
                await _context.ItemDetails.AddAsync(itemDetail);
                await _context.SaveChangesAsync();
                logger.LogInformation("ItemDetail added: {@ItemDetail}", itemDetail.CostDetails);
                return itemDetail;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail could not be added");
                return null;
            }
        }

        public async Task<bool> UpdateItemDetailAsync(ItemDetailModel itemDetail)
        {
            try
            {
                _context.ItemDetails.Update(itemDetail);
                await _context.SaveChangesAsync();
                logger.LogInformation("ItemDetail updated: {@ItemDetail}", itemDetail.CostDetails);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail {@ItemDetail} could not be updated", itemDetail.CostDetails);
                return false;
            }
        }

        public async Task<bool> DeleteItemDetailAsync(int id)
        {
            try
            {
                var itemDetail = await _context.ItemDetails.FindAsync(id);
                if (itemDetail == null) return false;
                _context.ItemDetails.Remove(itemDetail);
                await _context.SaveChangesAsync();
                logger.LogInformation("ItemDetail deleted: {@ItemDetail}", itemDetail.CostDetails);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail with {ID} could not be deleted", id );
                return false;
            }
        }
    }
}
