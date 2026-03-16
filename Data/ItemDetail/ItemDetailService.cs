using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.ItemDetail
{
    public class ItemDetailService(CashDataContext context, ILogger<ItemDetailService> logger,
        IStringLocalizer<Translation> localizer, IResultFactory operationResultFactory) : IItemDetailService
    {
        private string EntityName => localizer["ItemDetail"];

        public async Task<List<ItemDetailModel>> GetAllItemDetailsAsync(CancellationToken ct = default)
        {
            return await context.ItemDetails
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.Category)
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.CostCenter)
                .OrderByDescending(c => c.Id)
                .ToListAsync(ct);
        }

        public async Task<ItemDetailModel?> GetItemDetailByIdAsync(int id, CancellationToken ct = default)
        {
            return await context.ItemDetails
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.Category)
                .Include(ud => ud.Allocations)
                .ThenInclude(a => a.CostCenter)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<ItemDetailModel?> GetItemDetailByNameAsync(string name, CancellationToken ct = default)
        {
            return await  context.ItemDetails.FirstOrDefaultAsync(i => i.CostDetails == name, ct);
        }

        public async Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId, CancellationToken ct = default)
        {
            return await context.ItemDetails
                .Where(u => u.Allocations.Any(a => a.CategoryId == categoryId))
                .ToListAsync(ct);
        }

        public async Task<Result> AddItemDetailAsync(ItemDetailModel itemDetail, CancellationToken ct = default)
        {
            try
            {
                await context.ItemDetails.AddAsync(itemDetail, ct);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("ItemDetail added: {@ItemDetail}", itemDetail.CostDetails);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{itemDetail.CostDetails}'", itemDetail.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail could not be added");
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<Result> UpdateItemDetailAsync(ItemDetailModel itemDetail, CancellationToken ct = default)
        {
            try
            {
                context.ItemDetails.Update(itemDetail);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("ItemDetail updated: {@ItemDetail}", itemDetail.CostDetails);
                return operationResultFactory.SuccessUpdated(EntityName, itemDetail.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail {@ItemDetail} could not be updated", itemDetail.CostDetails);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<Result> DeleteItemDetailAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var itemDetail = await context.ItemDetails.FindAsync([id], ct);
                if (itemDetail == null)
                {
                    logger.LogWarning("ItemDetail with Id {ItemDetailId} not found", id);
                    return operationResultFactory.NotFound(EntityName, id);
                }
                context.ItemDetails.Remove(itemDetail);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("ItemDetail deleted: {@ItemDetail}", itemDetail.CostDetails);
                return operationResultFactory.SuccessDeleted($"{EntityName}: '{itemDetail.CostDetails}'", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ItemDetail with {ID} could not be deleted", id);
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}