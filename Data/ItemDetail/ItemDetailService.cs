using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.ItemDetail
{
    public class ItemDetailService(CashDataContext context, ILogger<ItemDetailService> logger,
        IStringLocalizer<Translation> localizer, IOperationResultFactory operationResultFactory) : IItemDetailService
    {
        private string EntityName => localizer["ItemDetail"];
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

        public async Task<ItemDetailModel?> GetItemDetailByNameAsync(string name)
        {
            return await  _context.ItemDetails.FirstOrDefaultAsync(i => i.CostDetails == name);
        }

        public async Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId)
        {
            return await _context.ItemDetails
                .Where(u => u.Allocations.Any(a => a.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task<IOperationResult> AddItemDetailAsync(ItemDetailModel itemDetail)
        {
            try
            {
                await _context.ItemDetails.AddAsync(itemDetail);
                await _context.SaveChangesAsync();
                logger.LogInformation("ItemDetail added: {@ItemDetail}", itemDetail.CostDetails);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{itemDetail.CostDetails}'", itemDetail.Id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "ItemDetail could not be added");
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> UpdateItemDetailAsync(ItemDetailModel itemDetail)
        {
            try
            {
                _context.ItemDetails.Update(itemDetail);
                await _context.SaveChangesAsync();
                logger.LogInformation("ItemDetail updated: {@ItemDetail}", itemDetail.CostDetails);
                return operationResultFactory.SuccessUpdated(EntityName, itemDetail.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail {@ItemDetail} could not be updated", itemDetail.CostDetails);
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> DeleteItemDetailAsync(int id)
        {
            try
            {
                var itemDetail = await _context.ItemDetails.FindAsync(id);
                if (itemDetail == null)
                {
                    logger.LogWarning("ItemDetail with Id {ItemDetailId} not found", id);
                    return operationResultFactory.NotFound(EntityName, id);
                }
                _context.ItemDetails.Remove(itemDetail);
                await _context.SaveChangesAsync();
                logger.LogInformation("ItemDetail deleted: {@ItemDetail}", itemDetail.CostDetails);
                return operationResultFactory.SuccessDeleted($"{EntityName}: '{itemDetail.CostDetails}'", id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "ItemDetail with {ID} could not be deleted", id );
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}
